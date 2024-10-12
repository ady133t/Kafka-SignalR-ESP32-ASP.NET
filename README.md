# Realtime ESP32->MQTT->Kafka->SignalR to ASP.NET Core MVC Dashboard
POC for demonstrating realtime communication between MQTT->Kafka->SignalR->ASP.Net web chart with VueJS integration (for quick DOM Manipulation)

# System Requirement
* .NET 6 and above
* Local or Cloud Kafka server 
* Custom MQtt Kafka Bridge solution-> https://github.com/ady133t/.NET-MQTT-Kafka-Bridge.git
* Public MQtt Broker -> https://www.emqx.com/en/mqtt/public-mqtt5-broker
* ESP32 Wokwi Simulation -> https://github.com/ady133t/ESP32-MQTT-PlatformIO-Wokwi.git

# Step to run
1) Run or connect to a kafka server 
2) Edit appsettings.json accordingly
3) Run the EF core migration to your database in this project.
4) Run this project > Task > Add device name as ESP32-01 (will be used as an kafka topic)
5) Edit appsettings.json from MQtt Kafka Source Connector solution to  setup Kafka,mqtt broker, SQL connection string
6) Run the Connector (preferable Container such as Docker)
7) Run your ESP32 or the Wokwi simulation with the code provided.

# Features
* EF Core (Code First)
* Dependency Injection (Background Service,EF DBContext, SignalR)
* Async Operation for handling multiple kafka topics for each devices
