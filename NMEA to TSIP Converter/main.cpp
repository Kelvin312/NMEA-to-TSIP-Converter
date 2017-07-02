/*
 * NEMA to TSIP Converter.cpp
 * ATmega328P 16 MHz
 * Created: 28.06.2017 7:30:19
 * Author : Andrey
 */ 

#include "stdafx.h"
#include "HardwareUART.cpp"
#include "SoftwareUART.cpp"
#include "NmeaParser.cpp"

#define LED_PIN 5
#define SUART_RX_PIN 3
#define SUART_TX_PIN 2
#define LED_PORT PORTB
#define SUART_RX_PORT PIND
#define SUART_TX_PORT PORTD

 SoftUart nmeaUart(SUART_RX_PORT,SUART_RX_PIN,SUART_TX_PORT,SUART_TX_PIN);
 HardUart tsipUart();
 RingBuffer<120> nmeaBuffer();
 NmeaParser parser();


ISR(TIMER1_CAPT_vect) //9600*3
{
  u8 data;
  if(nmeaUart.RxProcessing(data))
  {
	    nmeaBuffer.Push(data);
  }
}

void mainLoop()
{
  if(nmeaBuffer.Size())
  {
  parser.Parser(nmeaBuffer.Pop());
  }
}



int main()
{
  // Declare your local variables here
  
  // Crystal Oscillator division factor: 1
  clock_prescale_set(clock_div_1);

  // Input/Output Ports initialization
  DDRB=_BV(LED_PIN);
  PORTD=_BV(SUART_TX_PORT);
  DDRD=_BV(SUART_TX_PORT);

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
