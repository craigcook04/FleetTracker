#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function
#ifndef ARDUINO
    #define ARDUINO 10805
#endif
#ifndef ARDUINO_ARCH_SAMD
    #define ARDUINO_ARCH_SAMD
#endif
#include <Adafruit_GPS.h>
#include <MKRGSM.h>
#include <Adafruit_SleepyDog.h>
#include <cstring>

/* Header */

// Serial //

#define USB_SERIAL_ENBALED

#ifdef USB_SERIAL_ENBALED
    #define USB_PRINT(...) Serial.print(__VA_ARGS__)
    #define USB_PRINTLN(...) Serial.println(__VA_ARGS__)
    #define USB_WRITE(...) Serial.write(__VA_ARGS__)
    #define USB_INIT(speed) Serial.begin(speed)
#else
    #define USB_PRINT(...)
    #define USB_PRINTLN(...)
    #define USB_WRITE(...)
    #define USB_INIT(speed)
#endif

#define SERIAL_READ_TIMEOUT ((uint32_t)-1)
uint32_t readUInt32(Uart &serial, uint32_t timeoutDuration = 500);

Uart Serial3(&sercom4, 5, 4, SERCOM_RX_PAD_3, UART_TX_PAD_2);
void SERCOM4_Handler() { Serial3.IrqHandler(); }

extern void NVIC_SystemReset();

// GPS //

#define GPSSerial Serial1

Adafruit_GPS GPS(&GPSSerial);
uint32_t gpsTimer = millis();
uint32_t ultrasonicTimer = millis();

float angle  = -1;
bool newNME = false;


// Ultrasonic //

#define SIG_REQUEST (uint8_t)1

bool ultrasonicData = false;
bool ultrasonicFirst = true;
uint32_t ultrasonicCount = 0;
uint32_t ultrasonicEntered = 0;
uint32_t ultrasonicExited = 0;

void handleUltrasonic();


// Cell //

// Rogers SIM (Production)
const char GPRS_APN[]       = "internet.com";
const char GPRS_LOGIN[]     = "wapuser1";
const char GPRS_PASSWORD[]  = "wap";

// Bell SIM (Testing)
//const char GPRS_APN[]       = "pda.bell.ca";
//const char GPRS_LOGIN[]     = "";
//const char GPRS_PASSWORD[]  = "";

const char WEB_HOST[] = "sstrolley.ca";
const char WEB_PATH[] = "/api/trolley";
const int  WEB_PORT   = 80;

GSMClient *client;
GSM *gsm;
GPRS *gprs;

uint32_t lastSuccess;
uint32_t successes = 0;

char outBuf[1000];
size_t outBufLen = 0;

char headerBuf[1000];
size_t headerBufLen = 0;

char finalBuf[2000];

void writeToOutBuf(char *buffer, size_t &len, const char* buf, ...);
void resetOutBuf(size_t &len);

void deleteCell();
void createCell();
bool initGSM();
bool initGPRS();
void initCell();
void resetCell();
void hardReset();

void handleCell();


/* Implementation */

// Serial //

uint32_t readUInt32(Uart &serial, uint32_t timeoutDuration) {
    uint32_t val = 0;
    for (int i = 0; i < 4; i++) {
        Watchdog.reset();
        uint32_t timeoutTime = millis() + timeoutDuration;
        while (!Serial3.available()) {
            Watchdog.reset();
            if (millis() >= timeoutTime) {
                USB_PRINTLN("Serial read timeout");
                return SERIAL_READ_TIMEOUT;
            }
            delay(10);
        }
        val = (val >> 8) | (Serial3.read() << 24);
    }
    return val;
}


// Ultrasonic //

void handleUltrasonic() {
    if (millis() - ultrasonicTimer > 2000 && !ultrasonicData) {
        ultrasonicTimer = millis(); // Reset the ultrasonic timer
        Serial3.write(SIG_REQUEST);

        uint32_t val;

        val = readUInt32(Serial3);
        if (val == SERIAL_READ_TIMEOUT) {
            USB_PRINTLN("Ultrasonic serial read timeout 1");
            return;
        }
        ultrasonicCount = val;

        val = readUInt32(Serial3);
        if (val == SERIAL_READ_TIMEOUT) {
            USB_PRINTLN("Ultrasonic serial read timeout 2");
            return;
        }
        ultrasonicEntered = val;

        val = readUInt32(Serial3);
        if (val == SERIAL_READ_TIMEOUT) {
            USB_PRINTLN("Ultrasonic serial read timeout 3");
            return;
        }
        ultrasonicExited = val;

        ultrasonicData = true;
        USB_PRINTLN(ultrasonicCount);
        USB_PRINTLN(ultrasonicEntered);
        USB_PRINTLN(ultrasonicExited);
    }
}


// Cell //

void writeToOutBuf(char *buffer, size_t &len, const char* buf, ...) {
   va_list args;
   va_start(args, buf);
   len += vsprintf(buffer + len, buf, args);
   va_end(args);
}

void resetOutBuf(size_t &len) {
    len = 0;
}

bool initGSM() {
    USB_PRINT("Starting GSM ... ");
    Watchdog.reset();
    gsm->begin((const char *)0, true, false);
    uint32_t timeoutTime = millis() + 40000;
    USB_PRINTLN(timeoutTime);
    bool notTimeout = true;
    Watchdog.reset();
    while (!gsm->ready() && (notTimeout = (millis() < timeoutTime))) {
        USB_PRINTLN(millis());
        Watchdog.reset();
        delay(100);
    }
    if (!notTimeout) {
        USB_PRINTLN("Could not start GSM");
        return false;
    }
    USB_PRINTLN("GSM Started");
    return true;
}

bool initGPRS() {
    USB_PRINT("Starting GPRS ... ");
    uint32_t timeoutTime = millis() + 40000;
    USB_PRINTLN(timeoutTime);
    bool notTimeout = true;
    Watchdog.reset();
    gprs->attachGPRS(GPRS_APN, GPRS_LOGIN, GPRS_PASSWORD, false);
    Watchdog.reset();
    while (!gprs->ready() && (notTimeout = (millis() < timeoutTime))) {
        USB_PRINTLN(millis());
        Watchdog.reset();
        delay(100);
    }
    if (!notTimeout) {
        USB_PRINTLN("Could not start GPRS");
        return false;
    }
    USB_PRINTLN("GPRS started");
    return true;
}

void createCell() {
    client = new GSMClient(false);
    gprs = new GPRS();
    gsm = new GSM();
}

void deleteCell() {
    delete client;
    delete gprs;
    delete gsm;
}

void initCell() {
    USB_PRINTLN("Initilizing cell");
    bool first = true;
    uint32_t timeout = millis() + (4 * 60 * 1000);

    begin:

    if (millis() >= timeout)
        hardReset();

    Watchdog.reset();

    if (!first) deleteCell();
    delay(2000);
    first = false;
    Watchdog.reset();
    createCell();
    delay(2000);

    if (!initGSM()) goto begin;
    delay(2000);
    if (!initGPRS()) goto begin;

    USB_PRINTLN("Cell initilized");
}

void resetCell() {
    USB_PRINTLN("Resetting cell");
    newNME = false;
    successes = 0;
    uint32_t timeout = millis() + (3 * 60 * 1000);

    begin:
    
    if (millis() >= timeout)
        hardReset();

    Watchdog.reset();

    deleteCell();
    delay(2000);
    Watchdog.reset();
    createCell();
    delay(2000);

    if (!initGSM()) goto begin;
    delay(2000);
    if (!initGPRS()) goto begin;

    USB_PRINTLN("Cell reset");
}

void hardReset() {
    NVIC_SystemReset();

    while (true);
}

void handleCell() {
    if (millis() > lastSuccess + (6 * 60 * 1000)) {
        hardReset();
    }

    {
        char c;
        uint32_t timeout = millis() + 1200;
        do {
            c = GPS.read();
            //if (c) USB_WRITE(c);

            if (GPS.newNMEAreceived()) {
                //USB_PRINTLN("\nNew NME Arrived");
                if (!GPS.parse(GPS.lastNMEA())) { // this also sets the newNMEAreceived() flag to false
                    //USB_PRINTLN("Failed to parse NME");
                    return; // we can fail to parse a sentence in which case we should just wait for another
                }

                newNME = true;
            }
        } while (c > 0 && millis() < timeout);
    }

    if (millis() - gpsTimer > 2000) {
        gpsTimer = millis(); // Reset the cell timer
        USB_PRINT("\n\nTime: ");
        USB_PRINT(GPS.hour, DEC); USB_PRINT(':');
        USB_PRINT(GPS.minute, DEC); USB_PRINT(':');
        USB_PRINT(GPS.seconds, DEC); USB_PRINT('.');
        USB_PRINTLN(GPS.milliseconds);
        USB_PRINT("Date: ");
        USB_PRINT(GPS.day, DEC);USB_PRINT('/');
        USB_PRINT(GPS.month, DEC); USB_PRINT("/20");
        USB_PRINTLN(GPS.year, DEC);
        USB_PRINT("Fix: "); USB_PRINT((int)GPS.fix);
        USB_PRINT(" quality: "); USB_PRINTLN((int)GPS.fixquality);
        if (GPS.fix && (abs(GPS.latitude_fixed) >= 10000000 && abs(GPS.longitude_fixed) >= 10000000) && newNME) {
            newNME = false;
            USB_PRINT("Location: ");
            USB_PRINT(GPS.latitude_fixed / 10000000.0, 4); USB_PRINT(GPS.lat);
            USB_PRINT(", ");
            USB_PRINT(GPS.longitude_fixed / 10000000.0, 4); USB_PRINTLN(GPS.lon);
            USB_PRINT("Speed (knots): "); USB_PRINTLN(GPS.speed);
            USB_PRINT("Angle: "); USB_PRINTLN(GPS.angle);
            USB_PRINT("Altitude: "); USB_PRINTLN(GPS.altitude);
            USB_PRINT("Satellites: "); USB_PRINTLN((int)GPS.satellites);

            if (GPS.speed > 3) {
                angle = GPS.angle;
            }
            USB_PRINT("Real Angle: "); USB_PRINTLN(angle);

            resetOutBuf(outBufLen);
            resetOutBuf(headerBufLen);

            uint32_t lonFixed = GPS.longitude_fixed * (GPS.lon == 'E' ? 1 : -1);
            uint32_t latFixed = GPS.latitude_fixed * (GPS.lat == 'N' ? 1 : -1);

            writeToOutBuf(outBuf, outBufLen, "{\r\n\t\"Id\": 1,\r\n\t\"Login\": [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ],\r\n\t\"LonFixed\": ");
            writeToOutBuf(outBuf, outBufLen, "%d", lonFixed);
            writeToOutBuf(outBuf, outBufLen, ",\r\n\t\"LatFixed\": ");
            writeToOutBuf(outBuf, outBufLen, "%d", latFixed);
            writeToOutBuf(outBuf, outBufLen, ",\r\n\t\"Heading\": ");
            writeToOutBuf(outBuf, outBufLen, "%f", angle);

            if (ultrasonicData) {
                ultrasonicData = false;

                if (ultrasonicEntered > 0 || ultrasonicExited > 0 || ultrasonicFirst) {
                    ultrasonicFirst = false;
                    writeToOutBuf(outBuf, outBufLen, ",\r\n\t\"Passengers\":\r\n\t{\r\n\t\t\"Passengers\": ");
                    writeToOutBuf(outBuf, outBufLen, "%d", ultrasonicCount);
                    writeToOutBuf(outBuf, outBufLen, ",\r\n\t\t\"Arrived\": ");
                    writeToOutBuf(outBuf, outBufLen, "%d", ultrasonicEntered);
                    writeToOutBuf(outBuf, outBufLen, ",\r\n\t\t\"Departed\": ");
                    writeToOutBuf(outBuf, outBufLen, "%d", ultrasonicExited);
                    writeToOutBuf(outBuf, outBufLen, "\r\n\t}");
                }
            }

            writeToOutBuf(outBuf, outBufLen, "\r\n}");

            writeToOutBuf(headerBuf, headerBufLen, "POST ");
            writeToOutBuf(headerBuf, headerBufLen, WEB_PATH);
            writeToOutBuf(headerBuf, headerBufLen, " HTTP/1.1\r\n");
            writeToOutBuf(headerBuf, headerBufLen, "Host: ");
            writeToOutBuf(headerBuf, headerBufLen, WEB_HOST);
            writeToOutBuf(headerBuf, headerBufLen, "\r\n");
            writeToOutBuf(headerBuf, headerBufLen, "Content-Type: application/json\r\n");
            writeToOutBuf(headerBuf, headerBufLen, "Content-Length: ");
            writeToOutBuf(headerBuf, headerBufLen, "%lu", (unsigned long)outBufLen);
            writeToOutBuf(headerBuf, headerBufLen, "\r\n\r\n");

            memcpy(finalBuf, headerBuf, headerBufLen * sizeof(char));
            memcpy(finalBuf + headerBufLen, outBuf, outBufLen * sizeof(char));

            USB_PRINTLN("\r\nconnecting...");

            uint32_t timeout = millis() + 10000;
            while (client->connect(WEB_HOST, WEB_PORT) != 1) {
                Watchdog.reset();
                if (millis() >= timeout) {
                    USB_PRINTLN("Client not ready to connect");
                    resetCell();
                    return;
                }
                delay(100);
            }

            delay(500);

            client->beginWrite(false);
            uint8_t failCount = 0;
            uint32_t written = 0;
            const uint32_t writeTotal = (outBufLen + headerBufLen) * sizeof(char);
            USB_WRITE(finalBuf, writeTotal);
            USB_PRINTLN();
            while (written < writeTotal) {
                Watchdog.reset();
                uint32_t timeout = millis() + 5000;
                while (!client->ready()) {
                    Watchdog.reset();
                    if (millis() >= timeout) {
                        USB_PRINTLN("Failed to ready for GSM write");

                        client->endWrite();
                        client->stop();
                        
                        resetCell();
                        return;
                    }
                    delay(100);
                }
                uint32_t w = client->write((const uint8_t *)finalBuf + written, (writeTotal - written) > 255 ? 255 : (writeTotal - written));
                written += w;
                USB_PRINTLN(w);

                if (!w) {
                    USB_PRINTLN("GSM write failed");
                    
                    client->endWrite();
                    client->stop();
                    
                    resetCell();
                    return;
                }

                delay(1);
            }
            client->endWrite();

            if (++successes >= 2) {
                lastSuccess = millis();
                USB_PRINTLN(lastSuccess);
            }
            
            USB_PRINTLN();
            USB_PRINTLN("disconnecting.");

            delay(10);

            client->stop();
        }
    }
}


// Main //

void setup() {
    USB_INIT(115200);
    Serial3.begin(2400);

    Watchdog.enable(16000);
    lastSuccess = millis();
    successes = 0;

    pinPeripheral(4, PIO_SERCOM_ALT);
    pinPeripheral(5, PIO_SERCOM_ALT);

    GPS.begin(9600);
    GPS.sendCommand(PMTK_SET_NMEA_OUTPUT_RMCGGA);
    GPS.sendCommand(PMTK_ENABLE_SBAS);
    GPS.sendCommand(PMTK_ENABLE_WAAS);
    GPS.sendCommand(PMTK_SET_NMEA_UPDATE_1HZ); // 1 Hz update rate
    //GPS.sendCommand(PGCMD_ANTENNA); // Antenna status updates

    Watchdog.reset();

    delay(1000);

    // Ask for firmware version
    GPSSerial.println(PMTK_Q_RELEASE);

    Watchdog.reset();

    initCell();
}

void loop() {
    handleUltrasonic();
    handleCell();
    Watchdog.reset();

    delay(1);
}
