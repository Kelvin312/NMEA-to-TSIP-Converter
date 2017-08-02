/*
 * NEMA to TSIP Converter
 * ATmega328P 16 MHz
 * Created: 28.06.2017 7:30:19
 */ 

#include "stdafx.h"
#include "HardwareUART.cpp"
#include "SoftwareUART.cpp"
#include "NmeaParser.cpp"

//Настройки
#define GPS_UART_RX_PIN _BV(3) //Смещение программного RX, к которому подключен GPS TX
#define GPS_UART_TX_PIN _BV(4) //Смещение программного TX, к которому подключен GPS RX
#define PPS_PIN _BV(2)	//Смещение пина, к которому подключен PPS
SoftUart nmeaUart = SoftUart(PIND, GPS_UART_RX_PIN, PORTD, GPS_UART_TX_PIN); //Программный UART
HardUart tsipUart = HardUart(9600, ParityAndStop::Odd1); //Аппаратный UART
RingBuffer<128> nmeaBuffer = RingBuffer<128>(); //Кольцевой буфер пакетов NMEA
RingBuffer<64> tsipBuffer = RingBuffer<64>(); //Кольцевой буфер пакетов TSIP

//См. описание WaitAndTransmit
inline void TsipPushRaw(u8 data)
{
	tsipBuffer.Push(data);
}
NmeaParser parser = NmeaParser(&TsipPushRaw); //Преобразователь пакетов NMEA в TSIP

//Выбор флага прерывания в зависимости от смещения пина PPS
#if PPS_PIN == _BV(3) 
#define PPS_FLAG _BV(INTF1)
#else
#define PPS_FLAG _BV(INTF0)
#endif



volatile u16 parseTime;
static u8 EEMEM eepromResetCount[4];
struct SDebugInfo
{
	u8 powerOnResetCount;
	u8 externalResetCount;
	u8 brownOutResetCount;
	u8 watchdogResetCount;

	u8 parseErrorCount;
	u8 checkSumErrorCount;
	u16 lastErrorMsgId;

	u16 maxParseTime;
	u8 BufferOverflowCount;
	u8 resetFlags;
	u16 nDebugMessage;

	const u8 size = 4+4+3*2;
	SDebugInfo()
	{
		parseErrorCount=0;
		checkSumErrorCount=0;
		lastErrorMsgId=0;
		maxParseTime=0;
		BufferOverflowCount=0;
		nDebugMessage=0;
	}

	void ResetSourceCalc()
	{
		resetFlags = MCUSR;
		MCUSR = 0;

		if(resetFlags & _BV(PORF))
		{
			powerOnResetCount++;
		}
		if(resetFlags & _BV(EXTRF))
		{
			externalResetCount++;
		}
		if(resetFlags & _BV(BORF))
		{
			brownOutResetCount++;
		}
		if(resetFlags & _BV(WDRF))
		{
			watchdogResetCount++;
		}
	}

	void CheckBufferOverflow()
	{
		if(nmeaBuffer.isOverflow)
		{
			BufferOverflowCount += 0x01;
			nmeaBuffer.isOverflow = false;
		}
		if(tsipBuffer.isOverflow)
		{
			BufferOverflowCount += 0x10;
			tsipBuffer.isOverflow = false;
		}
	}

	void CheckParse(ErrorCode returnCode)
	{
		if(returnCode == ErrorCode::Error)
		{
			parseErrorCount++;
			lastErrorMsgId = parser.msgId;
		}
		if(returnCode == ErrorCode::CheckSumError)
		{
			checkSumErrorCount++;
			lastErrorMsgId = parser.msgId;
		}
	}

	void CalcParseTime(u16 curParseTime)
	{
		if(curParseTime > maxParseTime) maxParseTime = curParseTime;
	}
} debugInfo;

//Функция отправляет диагностическую информацию
void DebugSend()
{
	debugInfo.CheckBufferOverflow();
	TsipPushRaw(0xAA);
	TsipPushRaw(0xB1);
	for(u8 *ptr = reinterpret_cast<u8*>(&debugInfo); ptr < ptr+debugInfo.size; ++ptr)
	{
		TsipPushRaw(*ptr);
	}
	TsipPushRaw(0xAA);
	TsipPushRaw(0xB0);
	debugInfo.maxParseTime = 0;
}



//ppsTime5ms: Время, в 5мс интервалах, от фронта PPS
//ppsTime1s: Номер текущего PPS
//timer5ms: Время, в 5мс интервалах, от предыдущей отправки пакетов TSIP
//timerTick: Счетчик прерываний таймера для формирования 5мс интервалов
volatile u8 ppsTime5ms, ppsTime1s, timer5ms;
u8 timerTick; 

//Прерывание таймера с частотой 9600*3 Гц
ISR(TIMER1_CAPT_vect) 
{	
	u8 data; //Принятый байт
	if(nmeaUart.RxProcessing(data))
	{
		nmeaBuffer.Push(data);
	}
	if(tsipUart.TxProcessing() && !tsipBuffer.Empty())
	{
		tsipUart.Transmit(tsipBuffer.Pop());
	}

	if(--timerTick == 0)
	{
		timerTick = 144;
		++ppsTime5ms;
		if(++timer5ms == 0) timer5ms = 255;
	}

	if(EIFR & PPS_FLAG) //Проверка флага аппаратного прерывания по PPS
	{
		LED_PORT ^= LED_PIN;
		ppsTime5ms = 0;
		++ppsTime1s;
		EIFR |= PPS_FLAG;
	}

	++parseTime;
}

//ppsTimeOfDtFix: Номер PPS, когда был принят пакет с датой и временем
//time5s: Количество секунд, прошедшее с последней отправки пакета с GPS временем
//nPacketSend: Номер отправляемого пакета TSIP
//isDtFixOld: Флаг, для однократной установки ppsTimeOfDtFix в процессе приема и обработки пакета с датой и временем
u8 ppsTimeOfDtFix, time5s, nPacketSend;
bool isDtFixOld;

//Функция основного рабочего цикла
void MainLoop()
{
	if(!nmeaBuffer.Empty())
	{
		parseTime = 0;
		debugInfo.CheckParse(parser.Parse(nmeaBuffer.Pop()));
		debugInfo.CalcParseTime(parseTime);
		
		bool isDtFix = (parser.updateFlag & parser.UpdateDateTime) == parser.UpdateDateTime; //Флаг наличия флага parser.UpdateDateTime
		if(isDtFix && !isDtFixOld)
		{
			ppsTimeOfDtFix = ppsTime1s;
		}
		isDtFixOld = isDtFix;
	}
	
	//Если прошло > 990мс с последней посылки && < 100мс с начала PPS && мы не в середине разбора пакета
	if(timer5ms > 990/5 && ppsTime5ms < 100/5 && parser.dataType != parser.MsgData)
	{
		timer5ms = 0;
		++time5s;
		nPacketSend = 1;
	}
	if(nPacketSend > 0 && tsipBuffer.Size() < 64-(4*4*2+1+12+4))
	{
		switch(nPacketSend)
		{
			case 1: parser.PositionSend(); break;
			case 2: parser.VelocitySend(); break;
			case 3: if(time5s < 5) break;
				parser.gpsTime.DateTimeAdd(ppsTime1s - ppsTimeOfDtFix);
				parser.GpsTimeSend();
			break;
			case 4: if(time5s < 5) break;
				time5s = 0;
				parser.HealthSend();
			break;
			case 5: parser.SatelliteViewSend(); break;
			case 6: parser.FixModeSend(); break;
			case 7: if(time5s == 1) DebugSend(); break;
			default: nPacketSend = 0; break;
		}
		++nPacketSend;
	}
}


int main()
{
	//Установка делителя частоты кварца в 1
	clock_prescale_set(clock_div_1); 

	//Инициализация портов
	PORTB = 0x00; 
	DDRB = LED_PIN;

	PORTC=0x00;
	DDRC=0x00;

	PORTD = GPS_UART_TX_PIN | GPS_UART_RX_PIN | PPS_PIN;
	DDRD = GPS_UART_TX_PIN;
	
	//Выключить таймер 0
  TCCR0A=0x00;
  TCCR0B=0x00;
  TCNT0=0x00;
  OCR0A=0x00;
  OCR0B=0x00;

	//Таймер 1 в режиме CTC (обнуление таймера по совпадению)
  TCCR1A=0x00;
  TCCR1B=0x19;
  TCNT1H=0x00;
  TCNT1L=0x00;
	ICR1 = F_CPU / (9600UL * 3) - 1;
  OCR1AH=0x00;
  OCR1AL=0x00;
  OCR1BH=0x00;
  OCR1BL=0x00;

	//Выключить таймер 2
  ASSR=0x00;
  TCCR2A=0x00;
  TCCR2B=0x00;
  TCNT2=0x00;
  OCR2A=0x00;
  OCR2B=0x00;

	EICRA=0x0F; //Прерывания INT0 и INT1 по нарастающему фронту
	EIMSK=0x00; //Запретить вызов обработчика прерывания
	EIFR=0x03; //Обнулить флаги INTF1 INTF0 
	PCICR=0x00; //Выключить все PCINT

  // Timer/Counter 0 Interrupt(s) initialization
  TIMSK0=0x00;

  // Timer/Counter 1 Interrupt(s) initialization
  TIMSK1=0x20; //Включить вызов обработчика прерывания по CTC

  // Timer/Counter 2 Interrupt(s) initialization
  TIMSK2=0x00;

	//Выключить аналоговый компаратор
  ACSR=0x80;
  ADCSRB=0x00;
  DIDR1=0x00;

	//Выключить АЦП
  ADCSRA=0x00;

	//Выключить SPI
  SPCR=0x00;

	//Выключить TWI
  TWCR=0x00;

	//Сторожевой таймер с интервалом 120мс
  wdt_enable(WDTO_120MS);

	//Прочитать из EEPROM счетчики источников сброса, добавить текущий сброс и записать обратно
  eeprom_read_block(&debugInfo, eepromResetCount, 4);
  wdt_reset();
  debugInfo.ResetSourceCalc();
  eeprom_write_block(&debugInfo, eepromResetCount, 4);
  wdt_reset();

	//Разрешить глобально прервания
  sei();

	//Пакет с версией прошивки
	static const u8 softwareVersion[15] PROGMEM = {0x10, 0x45, 0x01, 0x10, 0x10, 0x02, 0x02, 0x06, 0x02, 0x19, 0x0C, 0x02, 0x05, 0x10, 0x03};
	for(u8 i=0; i<15; i++)
	{
		TsipPushRaw(pgm_read_byte(&softwareVersion[i]));
	}
	parser.HealthSend();
	wdt_reset();
	set_sleep_mode(SLEEP_MODE_IDLE); //Режим сна idle (в другом режиме прерывания по фронту не работают)

  while (1)
  {
    MainLoop();
	wdt_reset();
	sleep_enable();
	sleep_cpu(); //Спать
	sleep_disable();
  }
  return 0;
}
