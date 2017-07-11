/*
 * HardwareUART.cpp
 *
 * Created: 01.07.2017 18:34:34
 *  Author: Kelvin
 */ 

#ifndef HARD_UART_H_
#define HARD_UART_H_
#include "stdafx.h"

#define FRAMING_ERROR (1<<FE0)
#define PARITY_ERROR (1<<UPE0)
#define DATA_OVERRUN (1<<DOR0)
#define DATA_REGISTER_EMPTY (1<<UDRE0)
#define RX_COMPLETE (1<<RXC0)

class HardUart
{
	public:
	HardUart(u32 baudRate = 9600, ParityAndStop mode = ParityAndStop::None1)
	{
		//// USART initialization
		//// Communication Parameters: 8 Data, 1 Stop, No Parity
		//// USART Receiver: On
		//// USART Transmitter: On
		//// USART0 Mode: Asynchronous
		//// USART Baud Rate: 57600
		//UCSR0A=0x00;
		//UCSR0B=0x18;
		//UCSR0C=0x06;
		//UBRR0H=0x00;
		//UBRR0L=0x10;
		
		// USART initialization
		// Communication Parameters: 8 Data, ? Parity, ? Stop
		// USART Receiver: On
		// USART Transmitter: On
		// USART0 Mode: Asynchronous
		// USART Baud Rate: ?
		UCSR0A=0x00;
		UCSR0B=0x18;
		UCSR0C = (u8(mode) << 3) | 6;
		UBRR0 = F_CPU / (baudRate * 16UL) - 1;
	}

	bool RxProcessing(u8 &data)
	{
		char status;
		if (((status=UCSR0A) & RX_COMPLETE) == 0) return false;
		data = UDR0;
		return (status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN)) == 0;
	}
	
	bool TxProcessing()
	{
		return (UCSR0A & DATA_REGISTER_EMPTY) != 0;
	}
	
	void Transmit(u8 data)
	{
		UDR0 = data;
	}
	
	void TransmitAndWait(u8 data)
	{
		while((UCSR0A & DATA_REGISTER_EMPTY)==0);
		UDR0 = data;
	}
};

#endif /* HARD_UART_H_ */