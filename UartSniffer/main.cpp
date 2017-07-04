/*
 * UsrtSniffer.cpp
 *
 * Created: 04.07.2017 9:18:58
 * Author : Andrey
 */ 

#include <avr/io.h>
#include "stdafx.h"
#include "SoftwareUART.cpp"
#include "HardwareUART.cpp"

#define LED_PIN 5
#define LED_PORT PORTB
#define SUARTA_RX_PIN 2
#define SUARTB_RX_PIN 3
#define SUART_RX_PORT PIND

SoftUart suartA = SoftUart(SUART_RX_PORT,SUARTA_RX_PIN,PORTD,7);
SoftUart suartB = SoftUart(SUART_RX_PORT,SUARTB_RX_PIN,PORTD,7);
HardUart outuart = HardUart();
RingBuffer<120> aBuffer = RingBuffer<120>();
RingBuffer<120> bBuffer = RingBuffer<120>();
RingBuffer<120> outBuffer = RingBuffer<120>();

#define DLE 0x10
#define ETX 0x03

volatile u8 aDetect, bDetect;

ISR(TIMER1_CAPT_vect) //9600*3
{
	u8 data;
	if(suartA.RxProcessing(data))
	{
		aBuffer.Push(data);
		if(data == DLE)
		{
			aDetect = 1;
		}
		else
		{
			if((aDetect == 1 && data == ETX) || aDetect == 2) aDetect = 2;
			else aDetect = 0;
		}
	}
	if(suartB.RxProcessing(data))
	{
		bBuffer.Push(data);
		if(data == DLE)
		{
			bDetect = 1;
		}
		else
		{
			if((bDetect == 1 && data == ETX) || bDetect == 2) bDetect = 2;
			else bDetect = 0;
		}
	}
	if(outuart.TxProcessing())
	{
		if(outBuffer.Size()) outuart.Transmit(outBuffer.Pop());
	}
}

void mainLoop()
{
	if(aDetect == 2)
	{
		outBuffer.Push(0xAA);
		outBuffer.Push(0x00);
		while(aBuffer.Size())
		{
			outBuffer.Push(aBuffer.Pop());
		}
		aDetect = 0;
	}
	if(bDetect == 2)
	{
		outBuffer.Push(0xBB);
		outBuffer.Push(0x00);
		while(bBuffer.Size())
		{
			outBuffer.Push(bBuffer.Pop());
		}
		bDetect = 0;
	}
}



int main()
{
	// Declare your local variables here
	
	// Crystal Oscillator division factor: 1
	clock_prescale_set(clock_div_1);

	// Input/Output Ports initialization
	DDRB=_BV(LED_PIN);

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
	// INT0: Off
	// INT1: Off
	// Interrupt on any change on pins PCINT0-7: Off
	// Interrupt on any change on pins PCINT8-14: Off
	// Interrupt on any change on pins PCINT16-23: Off
	EICRA=0x00;
	EIMSK=0x00;
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


