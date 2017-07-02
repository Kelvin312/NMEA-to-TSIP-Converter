/*
 * HardwareUART.cpp
 *
 * Created: 01.07.2017 18:34:34
 *  Author: Kelvin
 */ 
#include "stdafx.h"


class HardUart
{
  public:
 #define FRAMING_ERROR (1<<FE0)
 #define PARITY_ERROR (1<<UPE0)
 #define DATA_OVERRUN (1<<DOR0)
 #define DATA_REGISTER_EMPTY (1<<UDRE0)
 #define RX_COMPLETE (1<<RXC0)

 HardUart()
 {
   // USART initialization
   // Communication Parameters: 8 Data, 1 Stop, Odd Parity
   // USART Receiver: On
   // USART Transmitter: On
   // USART0 Mode: Asynchronous
   // USART Baud Rate: 9600
   UCSR0A=0x00;
   UCSR0B=0x18;
   UCSR0C=0x36;
   UBRR0H=0x00;
   UBRR0L=0x67;
 }


 inline bool RxProcessing(u8 &data)
 {
   char status;
   if (((status=UCSR0A) & RX_COMPLETE)==0) return false;
   data = UDR0;
   if ((status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN))==0) return true;
   return false;
 }
 
 inline bool TxProcessing(u8 data)
 {
   if((UCSR0A & DATA_REGISTER_EMPTY)==0) return false;
   UDR0 = data;
   return true;
 }
};