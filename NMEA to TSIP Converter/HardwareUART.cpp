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
	//Конструктор
	//baudRate: скорость UART, бод
	//mode: режим работы UART
	HardUart(u32 baudRate = 9600, ParityAndStop mode = ParityAndStop::None1)
	{
		UCSR0A=0x00;
		UCSR0B=0x18;
		UCSR0C = (u8(mode) << 3) | 6;
		UBRR0 = F_CPU / (baudRate * 16UL) - 1;
	}

	//Финкция принимает байт по UART
	//возвращает true, если байт успешно принят
	//data: принятый байт
	bool RxProcessing(u8 &data)
	{
		char status; //Статус UART
		if (((status=UCSR0A) & RX_COMPLETE) == 0) return false;
		data = UDR0;
		return (status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN)) == 0;
	}
	
	//Функция возвращает true, если UART готов к передаче
	bool TxProcessing()
	{
		return (UCSR0A & DATA_REGISTER_EMPTY) != 0;
	}
	
	//Функция передает байт по UART
	//data: передаваемый байт
	void Transmit(u8 data)
	{
		UDR0 = data;
	}
	
	//Функция ждет готовности UART к передаче, а затем передает байт
	//data: передаваемый байт
	void WaitAndTransmit(u8 data)
	{
		while((UCSR0A & DATA_REGISTER_EMPTY)==0);
		UDR0 = data;
	}
};

#endif /* HARD_UART_H_ */