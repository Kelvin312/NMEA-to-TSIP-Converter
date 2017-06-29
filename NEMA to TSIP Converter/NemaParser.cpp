/*
 * NemaParser.cpp
 *
 * Created: 29.06.2017 10:11:43
 *  Author: Andrey
 */ 

#include "stdafx.h"
#include "IUart.h"

class NemaParser
{
	public:
	static const char ggaId[];
	static const char ggaIdSz = sizeof(ggaId)-1;

	void ParserProcessing(IUart &uart)
	{
		if(uart.IsTerminateMsg())
		{
			while(uart.NoEmpty() && uart.FrontAndPop() != '$');
			u8 msgSize = uart.Size();
			u8 checksum = 0;
			for(u8 i = 0; i < msgSize; i++)
			{
				u8 data = uart.Front();
				if(i < ggaIdSz)
				{
					if(ggaId[i] != data) return;
				}
				else
				{
					
				}
				checksum ^= data;
				uart.Pop();
			}
			
			
		}
	}
	
	
	
};

const char NemaParser::ggaId[] = "GPGGA,";
