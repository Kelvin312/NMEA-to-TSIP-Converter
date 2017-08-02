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

//См. описание WaitAndTransmit
inline void TsipPushRaw(u8 data)
{
	tsipUart.WaitAndTransmit(data);
}
NmeaParser parser = NmeaParser(&TsipPushRaw); //Преобразователь пакетов NMEA в TSIP

//Выбор флага прерывания в зависимости от смещения пина PPS
#if PPS_PIN == _BV(3) 
#define PPS_FLAG _BV(INTF1)
#else
#define PPS_FLAG _BV(INTF0)
#endif

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
}

//ppsTimeOfDtFix: Номер PPS, когда был принят пакет с датой и временем
//time5s: Количество секунд, прошедшее с последней отправки пакета с GPS временем
//isDtFixOld: Флаг, для однократной установки ppsTimeOfDtFix в процессе приема и обработки пакета с датой и временем
u8 ppsTimeOfDtFix, time5s;
bool isDtFixOld; 

//Функция основного рабочего цикла
void MainLoop()
{
	if(!nmeaBuffer.Empty())
	{
		wdt_reset();
		parser.Parse(nmeaBuffer.Pop());
		
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
		wdt_reset();
		parser.PositionAndVelocitySend();
		if(++time5s >= 5)
		{
			time5s = 0;
			wdt_reset();
			parser.gpsTime.DateTimeAdd(ppsTime1s - ppsTimeOfDtFix);
			parser.GpsTimeSend();
			parser.HealthSend();
		}
		wdt_reset();
		parser.SatelliteViewSend();
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
	sleep_cpu(); //спать
	sleep_disable();
  }
  return 0;
}
