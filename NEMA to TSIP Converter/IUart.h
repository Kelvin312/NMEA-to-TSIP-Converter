/*
* IUart.h
*
* Created: 29.06.2017 9:48:37
* Author: Andrey
*/


#ifndef __IUART_H__
#define __IUART_H__
#include "stdafx.h"

class IUart
{
	//functions
	public:
	//	virtual ~IUart(){}
	virtual u8 FrontAndPop() = 0;
	virtual u8 Size() = 0;
	virtual bool NoEmpty() = 0;
	virtual u8 Front(u8 index) = 0;
	virtual void Clear() = 0;
	virtual void Push(u8 data) = 0;
	virtual void StartTransmit() = 0;
	virtual bool IsTerminateMsg() = 0;
	virtual u8 Front() = 0;
	virtual void Pop() = 0;
}; //IUart

#endif //__IUART_H__
