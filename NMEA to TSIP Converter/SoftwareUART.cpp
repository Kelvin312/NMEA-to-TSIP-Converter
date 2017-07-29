#ifndef SOFT_UART_H_
#define SOFT_UART_H_
#include "stdafx.h"

class SoftUart
{
	private:
	volatile u8 &rxPort; //Порт для программного RX
	volatile u8 &txPort; //Порт для программного TX
	const u8 rxPin, txPin; //Смещения RX и TX пинов
	const ParityAndStop mode; //Режим работы UART
	
	//Функция возвращает состояние RX пина
	inline u8 GetRxPin(){ return rxPort & rxPin; }
	//Функция устанавливает TX пин в 1
	inline void SetTxPinHigh(){ txPort |= txPin; }
	//Функция устанавливает TX пин в 0
	inline void SetTxPinLow(){ txPort &= ~txPin; } 
	
	enum
	{
		WaitStartBit,
		ReadData,
		WaitStopBit
	} rxFlag; //Флаг состояния приемника
	
	//timerRxCtr: Счетчик для деления частоты вызова функции
	//rxFrameCounter: Счетчик принятых битов
	//rxFrameBuffer: Буфер принятых бит данных
	//rxMask: Смещение принимаемого бита в байте
	//rxParityBit: Бит четности
	//rxFrameDataBits: Количество бит данных + бит четности
	u8 timerRxCtr, rxFrameCounter;
	u8 rxFrameBuffer, rxMask, rxParityBit;
	const u8 rxFrameDataBits = (mode == ParityAndStop::None1) ? 8 : 9;
	
	//transmitComplete: Флаг завершения передачи
	//timerTxCtr: Счетчик для деления частоты вызова функции
	//txFrameCounter: Счетчик переданных битов
	//txParityBit: Бит четности
	//txFrameBuffer: Буфер передаваемых бит
	//txFrameSize: Количество бит данных + бит четности + старт и стоп бит
	volatile bool transmitComplete; 
	u8 timerTxCtr;
	volatile u8 txFrameCounter, txParityBit; 
	volatile u16 txFrameBuffer;
	const u8 txFrameSize = (mode == ParityAndStop::None1) ? 10 : 11;
	
	public:
	//Конструктор
	//rxPort: Порт для программного RX
	//txPort: Порт для программного TX
	//rxPin, txPin: Смещения RX и TX пинов
	//mode: Режим работы UART
	SoftUart(volatile u8 &rxPort, u8 rxPin, volatile u8 &txPort, u8 txPin, ParityAndStop mode = ParityAndStop::None1):
	rxPort(rxPort),
	rxPin(rxPin), 
	txPort(txPort), 
	txPin(txPin), 
	mode(mode)
	{
		txFrameCounter = txFrameSize;
		transmitComplete = true;
		rxFlag = WaitStartBit;
	}
	
	//Функция принимает байт по программному UART
	//возвращает true, если байт успешно принят
	//data: принятый байт
	bool RxProcessing(u8 &data)
	{
		switch(rxFlag)
		{
			case WaitStartBit:
			if(GetRxPin() == 0)
			{
				timerRxCtr = 4;
				rxFlag = ReadData;
				rxFrameCounter = rxFrameDataBits;
				rxFrameBuffer = 0;
				rxMask = 1;
				rxParityBit = (mode == ParityAndStop::Odd1) ? 1:0;
			}
			break;
			case ReadData: //Принимаем 8 бит
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				if(GetRxPin())
				{
					rxFrameBuffer |= rxMask;
					rxParityBit ^= 1;
				}
				rxMask <<= 1;
				if(--rxFrameCounter == 0)
				{
					rxFlag = WaitStopBit;
				}
			}
			break;
			case WaitStopBit:
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				if(GetRxPin())
				{
					rxFlag = WaitStartBit;
					if((mode == ParityAndStop::Odd1 || mode == ParityAndStop::Even1) 
					&& rxParityBit) return false;
					data = rxFrameBuffer;
					return true;
				}
			}
			break;
		}
		return false;
	}

	//Функция подготавлевает передачу байта по программному UART
	//data: передаваемый байт
	void Transmit(u8 data) 
	{
		if(transmitComplete)
		{
			txParityBit = data ^ (data >> 4);
			txParityBit ^= txParityBit >> 2;
			txParityBit ^= txParityBit >> 1;
			if(mode == ParityAndStop::Odd1) txParityBit ^= 1;
			txFrameBuffer = (u16(data)<<1) | 0xC00; //Добавляем старт и стоп биты.
			if(mode == ParityAndStop::None1 || (txParityBit & 1)) txFrameBuffer |= 1<<9;
			txFrameCounter = txFrameSize;
			transmitComplete = false;
		}
	}

	//Функция передает байт по программному UART
	//возвращает true, если передача завершена
	bool TxProcessing()
	{
		if(transmitComplete) return true;
		if(--timerTxCtr == 0)
		{
			timerTxCtr = 3;
			if(txFrameBuffer & 1)
			{
				SetTxPinHigh();
			}
			else
			{
				SetTxPinLow();
			}
			txFrameBuffer >>= 1;
			if(--txFrameCounter == 0)
			{
				SetTxPinHigh();
				transmitComplete = true;
				return transmitComplete;
			}
			
		}
		return false;
	}
	
	//Функция ждет готовности UART к передаче, а затем подготавлевает передачу байта
	//data: передаваемый байт
	void WaitAndTransmit(u8 data)
	{
		while(!transmitComplete);
		Transmit(data);
	}

};

#endif /* SOFT_UART_H_ */