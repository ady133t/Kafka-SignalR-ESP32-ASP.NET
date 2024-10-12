# Realtime Kafka-SignalR-ESP32-ASP.NET
POC for demonstrating realtime communication between MQTT->Kafka->SignalR->ASP.Net web chart with VueJS integration (for quick DOM Manipulation)

# System Requirement
* .NET 6 and above
* Kafka server 
* Custom MQtt Kafka Source Connector solution-> 
* MQtt Broker -> 
* ESP32 or Wokwi Simulation ->

# Step to run
1) Run or connect to a kafka server 
2) Edit appsettings.json accordingly
3) Run the EF core migration to your database in this project.
4) Run this project > Task > Add device name as ESP32-01 (will be used as an kafka topic)
5) Edit appsettings.json from MQtt Kafka Source Connector solution to  setup Kafka,mqtt broker, SQL connection string
6) Run the Connector
7) Run your ESP32 or the Wokwi simulation with the code provided.

# Features
* EF Core (Code First)
* Dependency Injection (Background Service,DBContext, SignalR)
* Async Operation for multiple topics for each devices
