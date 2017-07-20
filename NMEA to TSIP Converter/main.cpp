/*
 * NEMA to TSIP Converter
 * ATmega328P 16 MHz
 * Created: 28.06.2017 7:30:19
 */ 

#include "stdafx.h"
#include "HardwareUART.cpp"
#include "SoftwareUART.cpp"
#include "NmeaParser.cpp"

//Config
#define GPS_UART_RX_PIN _BV(4)
#define GPS_UART_TX_PIN _BV(5)
#define PPS_PIN _BV(3)
SoftUart nmeaUart = SoftUart(PIND, GPS_UART_RX_PIN, PORTD, GPS_UART_TX_PIN);
HardUart tsipUart = HardUart(9600, ParityAndStop::Odd1);
RingBuffer<128> nmeaBuffer = RingBuffer<128>();

inline void TsipPushRaw(u8 data)
{
	tsipUart.WaitAndTransmit(data);
}
NmeaParser parser = NmeaParser(&TsipPushRaw);

#if PPS_PIN == _BV(3)
#define PPS_FLAG _BV(INTF1)
#else
#define PPS_FLAG _BV(INTF0)
#endif

volatile u8 ppsTime5ms, ppsTime1s;
u8 ppsTimeOfDtFix;
u8 timerTick, timer5ms, timer996ms;
volatile enum
{
	OutPacketNone,
	OutPacket1s,
	OutPacket5s
} outPacketFlag;

ISR(TIMER1_CAPT_vect) //9600*3
{	
	u8 data;
	if(nmeaUart.RxProcessing(data))
	{
		nmeaBuffer.Push(data);
	}

	if(--timerTick == 0) //5.004ms
	{
		timerTick = 144;
		++ppsTime5ms;

		if(--timer5ms == 0) //995.796ms
		{
			timer5ms = 996/5;
			outPacketFlag = OutPacket1s;
			if(++timer996ms >= 5)
			{
				timer996ms = 0;
				outPacketFlag = OutPacket5s;
			}
		}
	}

	if(EIFR & PPS_FLAG) //PPS detect
	{
		LED_PORT ^= LED_PIN;
		ppsTime5ms = 0;
		++ppsTime1s;
		EIFR |= PPS_FLAG;
	}
}

void MainLoop()
{
	if(nmeaBuffer.Size())
	{
		parser.Parse(nmeaBuffer.Pop());
		if((parser.updateFlag & parser.UpdateDateTime) == parser.UpdateDateTime)
		{
			ppsTimeOfDtFix = ppsTime1s;
		}
	}
	if(outPacketFlag != OutPacketNone && ppsTime5ms > 0 && ppsTime5ms < 400/5)
	{
		parser.PositionVelocitySend();
		if(outPacketFlag == OutPacket5s)
		{
			timeCounter = 0;
			parser.DateTimeAdd(ppsTime1s - ppsTimeOfDtFix);
			parser.GpsTimeSend();
			parser.HealthSend();
		}
		parser.ViewSatelliteSend();
		outPacketFlag = OutPacketNone;
	}
}


int main()
{
	// Declare your local variables here
	
	// Crystal Oscillator division factor: 1
	clock_prescale_set(clock_div_1);

	// Input/Output Ports initialization
	PORTB = 0x00;
	DDRB = LED_PIN;

	PORTC=0x00;
	DDRC=0x00;

	PORTD = GPS_UART_TX_PIN | GPS_UART_RX_PIN | PPS_PIN;
	DDRD = GPS_UART_TX_PIN;

  // Timer/Counter 0 initialization
  // Clock source: System Clock
  // Clock value: Timer 0 Stopped
  // Mode: Normal top=0xFF
  // OC0A output: Disconnected
  // OC0B output: Disconnected
  TCCR0A=0x00;
  TCCR0B=0x00;
  TCNT0=0x00;
  OCR0A=0x00;
  OCR0B=0x00;

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
  ICR1H=0x02;
  ICR1L=0x2B;
  OCR1AH=0x00;
  OCR1AL=0x00;
  OCR1BH=0x00;
  OCR1BL=0x00;

  // Timer/Counter 2 initialization
  // Clock source: System Clock
  // Clock value: Timer2 Stopped
  // Mode: Normal top=0xFF
  // OC2A output: Disconnected
  // OC2B output: Disconnected
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
 EICRA=0x0F;
 EIMSK=0x00; //Прерывание
 EIFR=0x03; //INTF1 INTF0 
 PCICR=0x00;

  // Timer/Counter 0 Interrupt(s) initialization
  TIMSK0=0x00;

  // Timer/Counter 1 Interrupt(s) initialization
  TIMSK1=0x20;

  // Timer/Counter 2 Interrupt(s) initialization
  TIMSK2=0x00;

  // Analog Comparator initialization
  // Analog Comparator: Off
  // Analog Comparator Input Capture by Timer/Counter 1: Off
  ACSR=0x80;
  ADCSRB=0x00;
  DIDR1=0x00;

  // ADC initialization
  // ADC disabled
  ADCSRA=0x00;

  // SPI initialization
  // SPI disabled
  SPCR=0x00;

  // TWI initialization
  // TWI disabled
  TWCR=0x00;

  // Watchdog Timer initialization
  // Watchdog Timer Prescaler: OSC/16k
  // Watchdog Timer interrupt: Off
  wdt_enable(WDTO_120MS);

  // Global enable interrupts
  sei();

	static const u8 softwareVersion[15] PROGMEM = {0x10, 0x45, 0x01, 0x10, 0x10, 0x02, 0x02, 0x06, 0x02, 0x19, 0x0C, 0x02, 0x05, 0x10, 0x03};
	for(u8 i=0; i<15; i++)
	{
		TsipPushRaw(pgm_read_byte(&softwareVersion[i]));
		wdt_reset();
	}
	parser.HealthSend();
	wdt_reset();

  while (1)
  {
    MainLoop();
	wdt_reset();
  }
  return 0;
}
