EESchema Schematic File Version 4
LIBS:trolleySchematic-cache
EELAYER 29 0
EELAYER END
$Descr A4 11693 8268
encoding utf-8
Sheet 1 3
Title ""
Date ""
Rev ""
Comp ""
Comment1 ""
Comment2 ""
Comment3 ""
Comment4 ""
$EndDescr
Text Label 11250 8000 0    50   ~ 0
Box1
$Comp
L Connector:Screw_Terminal_01x04 J1
U 1 1 5CF66A8A
P 7200 5450
AR Path="/5CF66A8A" Ref="J1"  Part="1" 
AR Path="/5CF1EF40/5CF66A8A" Ref="J?"  Part="1" 
F 0 "J1" H 7280 5442 50  0000 L CNN
F 1 "Screw_Terminal_01x04" H 7280 5351 50  0000 L CNN
F 2 "" H 7200 5450 50  0001 C CNN
F 3 "~" H 7200 5450 50  0001 C CNN
	1    7200 5450
	1    0    0    -1  
$EndComp
Wire Wire Line
	7000 5350 6050 5350
Wire Wire Line
	6900 3750 6900 3500
Wire Wire Line
	5950 5450 7000 5450
Wire Wire Line
	5300 5000 6700 5000
Wire Wire Line
	6700 5000 6700 3500
Wire Wire Line
	5300 5100 6600 5100
Wire Wire Line
	6600 5100 6600 3500
Wire Wire Line
	6800 3850 6800 3500
Wire Wire Line
	4000 5700 3600 5700
Wire Wire Line
	3600 5700 3600 6300
Wire Wire Line
	3600 6300 7000 6300
Wire Wire Line
	7000 6300 7000 5650
Wire Wire Line
	4000 5800 3700 5800
Wire Wire Line
	3700 5800 3700 6200
Wire Wire Line
	3700 6200 6900 6200
Wire Wire Line
	6900 6200 6900 5550
Wire Wire Line
	6900 5550 7000 5550
Wire Wire Line
	5400 4800 5950 4800
Connection ~ 5400 4800
Wire Wire Line
	5300 4800 5400 4800
Wire Wire Line
	5400 3850 6800 3850
Wire Wire Line
	5400 4800 5400 3850
Wire Wire Line
	5950 4800 5950 5450
Connection ~ 5300 4500
Wire Wire Line
	5300 3750 6900 3750
Wire Wire Line
	5300 4500 5300 3750
Wire Wire Line
	6050 4500 5300 4500
Wire Wire Line
	6050 5350 6050 4500
$Comp
L trolleyParts:AdafruitGPS U?
U 1 1 5CF680B7
P 6500 3450
F 0 "U?" H 7128 4088 50  0000 L CNN
F 1 "AdafruitGPS" H 7128 3997 50  0000 L CNN
F 2 "" H 6600 3000 50  0001 C CNN
F 3 "" H 6600 3000 50  0001 C CNN
	1    6500 3450
	1    0    0    -1  
$EndComp
$Comp
L trolleyParts:ArduinoGSM1400 U?
U 1 1 5CF1E4C8
P 4650 5950
F 0 "U?" H 4650 8665 50  0000 C CNN
F 1 "ArduinoGSM1400" H 4650 8574 50  0000 C CNN
F 2 "" H 4650 6000 50  0001 C CNN
F 3 "" H 4650 6000 50  0001 C CNN
	1    4650 5950
	1    0    0    -1  
$EndComp
$Sheet
S 0    8600 11700 8250
U 5CF9DD85
F0 "Box 3" 50
F1 "file5CF9DD84.sch" 50
$EndSheet
$Sheet
S 12150 -50  11700 8250
U 5CF6B817
F0 "Box 2" 50
F1 "file5CF6B816.sch" 50
$EndSheet
$EndSCHEMATC
