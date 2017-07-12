/*
 * NEMA to TSIP Converter.cpp
 * ATmega328P 16 MHz
 * Created: 28.06.2017 7:30:19
 * Author : Andrey
 */ 

#include "stdafx.h"
#include "HardwareUART.cpp"
#include "SoftwareUART.cpp"


#define GPS_UART_RX_PIN _BV(2)
#define GPS_UART_TX_PIN _BV(6)
#define MG_UART_RX_PIN _BV(5)
#define MG_UART_TX_PIN _BV(4)

#define PPS_PIN _BV(3)
#define PPS_PORT PIND
//
//SoftUart gpsUart = SoftUart(PIND, GPS_UART_RX_PIN, PORTD, GPS_UART_TX_PIN, ParityAndStop::Odd1);
//SoftUart tsipUart = SoftUart(PIND, MG_UART_RX_PIN, PORTD, MG_UART_TX_PIN, ParityAndStop::Odd1); 
//HardUart debugUart = HardUart(9600, ParityAndStop::Odd1);
//
//RingBuffer<128> debugBuffer = RingBuffer<128>();
//RingBuffer<128> gpsBuffer = RingBuffer<128>();
//
//volatile u16 timeCounter, ppsTimeMSec;
//u8 timePrescaler;
//volatile u8 uartNumber = 0;
//
//inline void tsipSend(u8 data)
//{
	//cli();
	//if(uartNumber != 1)
	//{
		//uartNumber = 1;
		//debugBuffer.Push(0xAA);
		//debugBuffer.Push(0xB1);
	//}
	//debugBuffer.Push(data);
	//sei();
	//tsipUart.TransmitAndWait(data);
//}
//
//
//ISR(TIMER1_CAPT_vect) //9600*3
//{
	//if(++timePrescaler >= 29)
	//{
		//timePrescaler = 0; 
		//++timeCounter;
		//++ppsTimeMSec;
	//}
	//if(EIFR & _BV(INTF1)) //Нарастающий фронт
	//{
		////++parser.ppsTimeSec;
		//ppsTimeMSec = 0;
		//EIFR &= _BV(INTF1);
		//LED_PORT ^= LED_PIN;
	//}
	//if(PPS_PORT & PPS_PIN)
	//{
		//ppsTimeMSec = 0;
	//}
	//
	//u8 data;
	//if(gpsUart.RxProcessing(data))
	//{
		//gpsBuffer.Push(data);
	//}
	//if(tsipUart.TxProcessing())
	//{
		////
	//}
	//if(tsipUart.RxProcessing(data))
	//{
		//if(uartNumber != 0)
		//{
			//uartNumber = 0;
			//debugBuffer.Push(0xAA);
			//debugBuffer.Push(0xB0);
		//}
		//debugBuffer.Push(data);
	//}
	//if(debugUart.TxProcessing() && debugBuffer.Size())
	//{
		//debugUart.Transmit(debugBuffer.Pop());
	//}
//}

RingBuffer<128> gpsBuffer = RingBuffer<128>();

ISR(USART_RX_vect)
{
	char status,data;
	status=UCSR0A;
	data=UDR0;
	if ((status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN))==0)
	{
		gpsBuffer.Push(data);
	}
}

inline void tsipSend(u8 data)
{
	while((UCSR0A & DATA_REGISTER_EMPTY)==0);
	UDR0 = data;
}

static const u8 DLE = 0x10;
static const u8 ETX = 0x03;
enum
{
	tsipDLE,
	tsipStart,
	tsipEnd,
	tsipData
} tsipStatus = tsipEnd;
u8 commandId = 0, commandByte = 0;

void TsipParse(u8 data)
{
	//Замена байт
	if(commandId == 0x6D && commandByte == 1)
	{
		 tsipSend(0x64);
		 return;
	}
	if(commandId == 0x6D && commandByte == 17)
	{
		tsipSend(data);
		tsipSend(0x60);
		tsipSend(0x70);
		tsipSend(0x62);
		tsipSend(0x72);
		tsipSend(0x64);
		tsipSend(0x74);
		return;
	}
	if(commandId == 0x46 && commandByte == 1)
	{
		tsipSend(0x00);
		return;
	}
	//**********
	tsipSend(data);
}


void mainLoop()
{
	if(gpsBuffer.Size())
	{
		u8 c = gpsBuffer.Pop();
		++commandByte;
		switch(c)
		{
			case DLE:
			if(tsipStatus == tsipEnd)
			{
				tsipStatus = tsipStart;
			}
			else
			{
				tsipStatus = tsipDLE;
			}
			break;
			case ETX:
				if(tsipStatus == tsipDLE)
				{
					tsipStatus = tsipEnd;
				}
			break;
			default:
			if(tsipStatus == tsipStart)
			{
				commandByte = 0;
				commandId = c;
				tsipStatus = tsipData;
			}
			break;
		}
		if((tsipStatus == tsipData || tsipStatus == tsipDLE) && commandByte > 0)
		{
			TsipParse(c);
			return;
		}
		
		tsipSend(c);
	}
}




int main()
{
	// Declare your local variables here
	
	// Crystal Oscillator division factor: 1
	clock_prescale_set(clock_div_1);

// USART initialization
// USART disabled
//UCSR0B=0x00;

// USART initialization
// Communication Parameters: 8 Data, 1 Stop, Odd Parity
// USART Receiver: On
// USART Transmitter: On
// USART0 Mode: Asynchronous
// USART Baud Rate: 9600
UCSR0A=0x00;
UCSR0B=0x98;
UCSR0C=0x36;
UBRR0H=0x00;
UBRR0L=0x67;

	// Input/Output Ports initialization
	PORTB = 0x00;
	DDRB = LED_PIN;

	PORTC=0x00;
	DDRC=0x00;

	//PORTD = GPS_UART_TX_PIN | MG_UART_TX_PIN;
	//DDRD = GPS_UART_TX_PIN | MG_UART_TX_PIN;
	PORTD = 0;
	DDRD = 0;

  //PORTD=_BV(SUART_TX_PORT);
  //DDRD=_BV(SUART_TX_PORT);

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
  //TCCR1A=0x00;
  //TCCR1B=0x19;
  //TCNT1H=0x00;
  //TCNT1L=0x00;
  //ICR1H=0x02;
  //ICR1L=0x2B;
  //OCR1AH=0x00;
  //OCR1AL=0x00;
  //OCR1BH=0x00;
  //OCR1BL=0x00;

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
 // INT0: Off
 // INT1: On
 // INT1 Mode: Rising Edge
 // Interrupt on any change on pins PCINT0-7: Off
 // Interrupt on any change on pins PCINT8-14: Off
 // Interrupt on any change on pins PCINT16-23: Off
 EICRA=0x0C;
 EIMSK=0x00;
 EIFR=0x02; //INTF1: External Interrupt Flag 1
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

  while (1)
  {
    mainLoop();
	wdt_reset();
  }
  return 0;
}
