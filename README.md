# ETF Tracker

A c# console app that tracks real-time prices of selected ETFs available on Finnhub, using Finnhub API

## Features
- Fetches current prices of popular ETFs (e.g. QQQM, SPY, VOO)
- Uses `.env` for secure API key storage
- Alerts when an ETF drops more than 3% from the previous close (WIP)

## Setup

1. Install dependencies:
   ```bash
   dotnet add package dotenv.net
   dotnet add package Newtonsoft.Json

2. Create a .env file:
   FINNHUB_API_KEY=your_api_key_here

3. Run the project:
   dotnet run