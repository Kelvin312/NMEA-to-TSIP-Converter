# NMEA to TSIP Converter

* GPS Receivers
* New: CONDOR C2626 / NMEA 0183 (2.8V TXD Pin 5 are 9600 baud 8-None-1)
* Old: Lassen iQ / TSIP (3.3V TXD Pin 1 are 9600 baud 8-odd-1)
* Hardware
* Arduino Nano v3.0 (ATmega328 16МГц 5V)

### TSIP packet structure

    <0x10> <id> <data string bytes> <0x10> <0x03>
    <id> любое значение, кроме <0x10> и <0x03>.
    0x10 в данных надо экранировать добавлением 0x10 ('stuffing') (!!!)
    <0x10> <0x03> означает конец, только если количество <0x10> четно.
    Порядок байт - со старшего
    Single = Float
	
| | | |
|-------------------------------------------|----------------|----------|
| 0x41 										| GPS time	| 5 seconds|
| 0x42, 0x83, 0x4A(default), 0x84, 0x8F-20 	| position (choose packet with I/O options)	| 1 second |
| 0x43, 0x56(default), 0x8F-20				| velocity (choose packet with I/O options)	| 1 second |
| 0x46										| health of receiver	| 5 seconds |
| 0x4B										| machinecode/status (includes antenna fault detect)	| 5 seconds |
| 0x6D										| all-in-view satellite selection, DOPs, Fix Mode	| 1 second |
| 0x82										| DGPS position fix mode (only in DGPS mode)	| 1 second |

Get GPS time 0x21

Packet 0x41 - GPS Time 

| Byte	|			Item			 | Type	    | Units      |
|-------|----------------------------|----------|------------|
| 0-3	|	GPS time of week		 | 	Single	|  seconds   |
| 4-5	|	Extended GPS week number | 	INT16	|  weeks     |
| 6-9	|	GPS UTC offset			 | 	Single	|  seconds   |
* UTC = (GPS time) - (GPS UTC offset).
* Неделя №0 началась 6 января 1980 года.
* Счет секунд начинается с «0» Каждое воскресенье В полночь

Packet 0x46 - Health of Receiver

| Byte |	Item |	Value |
|------|---------|--------|
| 0	| Status code			| 0x00-0x0C                                                |
| 1	| Battery and Antenna | 0b00XX000X                                                 |
| 0x00 | Doing position fixes (full accuracy)                                          |
| 0x01 | Don't have GPS time yet (none/unknown)                                        |
| 0x02 | Need initialization (0=normal, 1=shutdown due to RF initialization timeout)   |
| 0x03 | PDOP is too high                                                              |
| 0x08 | No usable satellites                                                          |
| 0x09 | Only 1 usable satellite                                                       |
| 0x0A | Only 2 usable satellites                                                      |
| 0x0B | Only 3 usable satellites                                                      |
| 0x0C | The chosen satellite is unusable                                              |
Биты в 1 байте сбрасываются только ресетом.

### NMEA 0183 Message Format

    $GP<MSG><,D1,D2,D3,D4,.......,Dn>*<CS>[CR][LF]
    MSG - индентификатор сообщения
    “,” - разделители полей данных
    Dn - данные
    CS - HЁX контрольная XOR-сумма всех байт в строке между «$» и «*»

| | | |
|------|----------------|----------|
GGA	|	Default | GPS fix data |
GSA	|	Default | GPS DOP and active satellites |
GSV	|	Default | GPS satellites in view |
RMC |	Default | Recommended minimum specific GPS/Transit data   |
CHN	|	Other	|  GPS channel status                             |
GLL	|	Other	|  Geographic position – Latitude/Longitude       |
VTG	|	Other	|  Track Made Good and Ground Speed               |
ZDA	|	Other	|  Time and date                                  |

$GPGGA,hhmmss.ss,llll.lll,a,nnnnn.nnn,b,t,uu,v.v,w.w,M,x.x,M,y.y,zzzz*hh<CR><LF>

|   | |
|---|----------------|
1	| UTC of Position (when UTC offset has been decoded by the receiver)                                                                     |
2,3	| Latitude, N (North) or S (South)                                                                                                       |
4,5	| Longitude, E (East) or W (West)                                                                                                        |
6	| GPS Quality Indicator: 0 = No GPS, 1 = GPS, 2 = DGPS                                                                                   |
7	| Number of Satellites in Use                                                                                                            |
8	| Horizontal Dilution of Precision (HDOP)                                                                                                |
9, 10	| Antenna Altitude in Meters, M = Meters                                                                                             |
11, 12	| Geoidal Separation in Meters, M=Meters. Geoidal separation is the difference between the WGS-84 earth ellipsoid and mean-sea-level.|
13	| Age of Differential GPS Data. Time in seconds since the last Type 1 or 9 Update                                                        |
14	| Differential Reference Station ID (0000 to 1023)                                                                                       |
hh	| Checksum                                                                                                                               |

