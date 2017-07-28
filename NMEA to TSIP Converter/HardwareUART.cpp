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
	HardUart(u32 baudRate = 9600, ParityAndStop mode = ParityAndStop::None1) //Скорость, бод / бит четности и стоп бит
	{
		UCSR0A=0x00;
		UCSR0B=0x18;
		UCSR0C = (u8(mode) << 3) | 6;
		UBRR0 = F_CPU / (baudRate * 16UL) - 1;
	}

	bool RxProcessing(u8 &data) //Прием байта / флаг принятого байта / принятый байт
	{
		char status; //Статут UART
		if (((status=UCSR0A) & RX_COMPLETE) == 0) return false;
		data = UDR0;
		return (status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN)) == 0;
	}
	
	bool TxProcessing() //Определение возможности передать байт / флаг возможности передать байт
	{
		return (UCSR0A & DATA_REGISTER_EMPTY) != 0;
	}
	
	void Transmit(u8 data) //Передача байта / передаваемый байт
	{
		UDR0 = data;
	}
	
	void WaitAndTransmit(u8 data) //Ожидание возможности передачи байта и передача байта / передаваемый байт
	{
		while((UCSR0A & DATA_REGISTER_EMPTY)==0);
		UDR0 = data;
	}
};

#endif /* HARD_UART_H_ */