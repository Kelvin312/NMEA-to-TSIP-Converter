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
#define GPS_UART_RX_PIN _BV(4) //Номер программного RX, к которому подключен GPS TX
#define GPS_UART_TX_PIN _BV(5) //Номер программного TX, к которому подключен GPS RX
#define PPS_PIN _BV(3)	//Номер пина, к которому подключен PPS
SoftUart nmeaUart = SoftUart(PIND, GPS_UART_RX_PIN, PORTD, GPS_UART_TX_PIN); //Программный UART
HardUart tsipUart = HardUart(9600, ParityAndStop::Odd1); //Аппаратный UART
RingBuffer<128> nmeaBuffer = RingBuffer<128>(); //Кольцевой буфер пакетов NMEA

inline void TsipPushRaw(u8 data) //Отправка байта TSIP / отправляемый байт
{
	tsipUart.WaitAndTransmit(data);
}
NmeaParser parser = NmeaParser(&TsipPushRaw); //Преобразователь пакетов NMEA в TSIP

#if PPS_PIN == _BV(3) //Выбор флага прерывания в зависимости от номера пина
#define PPS_FLAG _BV(INTF1)
#else
#define PPS_FLAG _BV(INTF0)
#endif

volatile u8 ppsTime5ms, ppsTime1s, timer5ms; //Время, в 5мс интервалах, от фронта PPS / номер текущего PPS / время, в 5мс интервалах, от предыдущей отправки пакетов TSIP
u8 timerTick; //Счетчик прерываний таймера для формирования 5мс интервалов

ISR(TIMER1_CAPT_vect) //9600*3 //Прерывание таймера с частотой 9600*3 Гц
{	
	u8 data; //Принятый байт
	if(nmeaUart.RxProcessing(data))
	{
		nmeaBuffer.Push(data);
	}

	if(--timerTick == 0) //5.004ms
	{
		timerTick = 144;
		++ppsTime5ms;
		++timer5ms;
		 //995.796ms
	}

	if(EIFR & PPS_FLAG) //PPS detect //Проверка флага аппаратного прерывания по PPS
	{
		LED_PORT ^= LED_PIN;
		ppsTime5ms = 0;
		++ppsTime1s;
		EIFR |= PPS_FLAG;
	}
}

u8 ppsTimeOfDtFix, time5s; //Номер PPS, когда был принят пакет с датой и временем / количество секунд, прошедшее с последней отправки пакета с GPS временем
bool isDtFixOld; //Флаг для однократной установки ppsTimeOfDtFix в процессе приема и обработки пакета с датой и временем

void MainLoop() //Основной рабочий цикл
{
	if(nmeaBuffer.Size())
	{
		wdt_reset();
		parser.Parse(nmeaBuffer.Pop());
		
		bool isDtFix = (parser.updateFlag & parser.UpdateDateTime) == parser.UpdateDateTime; //Флаг флага parser.UpdateDateTime
		if(isDtFix && !isDtFixOld)
		{
			ppsTimeOfDtFix = ppsTime1s;
		}
		isDtFixOld = isDtFix;
	}
	
	//Если прошло > 990мс с последней посылки && < 200мс с начала PPS && мы не в середине разбора пакета
	if(timer5ms > 990/5 && ppsTime5ms < 200/5 && parser.dataType != parser.MsgData)
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
	// Declare your local variables here
	
	//Установка делителя частоты кварца в 1
	// Crystal Oscillator division factor: 1
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
  // Timer/Counter 1 initialization
  // Clock source: System Clock
  // Clock value: 16000,000 kHz
  // Mode: CTC top=ICR1
  // OC1A output: Discon.
  // OC1B output: Discon.
  // Noise Canceler: Off
  // Input Capture on Falling Edge
  // Timer1 Overflow Interrupt: Off
  // Input Capture Interrupt: On
  // Compare A Match Interrupt: Off
  // Compare B Match Interrupt: Off
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

 // External Interrupt(s) initialization
 // INT0 Mode: Rising Edge
 // INT1 Mode: Rising Edge
 // Interrupt on any change on pins PCINT0-7: Off
 // Interrupt on any change on pins PCINT8-14: Off
 // Interrupt on any change on pins PCINT16-23: Off
 EICRA=0x0F; //Прерывания INT0 и INT1 по нарастающему фронту
 EIMSK=0x00; //Запретить вызов обработчика прерывания
 EIFR=0x03; //Обнулить флаги INTF1 INTF0 
 PCICR=0x00;

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

	//Включить сторожевой таймер с интервалом 120мс
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
	if(nmeaBuffer.Empty()) sleep_cpu(); //Если буфер NMEA пустой, спать
	sleep_disable();
  }
  return 0;
}
