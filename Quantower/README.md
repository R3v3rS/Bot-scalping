# Quantower - Altcoin M1 Momentum Breakout

## 1) Jak dodać strategię do Quantower
1. Otwórz **Quantower** i moduł **Algo**.
2. Utwórz nową strategię C# (lub otwórz projekt z kodem).
3. Skopiuj plik `AltcoinM1MomentumBreakoutStrategy.cs` do projektu strategii.
4. Zbuduj projekt (`Build`).
5. W Quantower wybierz strategię **Altcoin M1 Momentum Breakout** i przypnij do wykresu **1m**.
6. Ustaw `Account`, `Symbol`, parametry oraz uruchom strategię.

## 2) Parametry startowe
- Fast EMA: `50`
- Slow EMA: `200`
- Breakout Lookback: `20`
- ATR Period: `14`
- ATR SMA Period: `100`
- Volume SMA Period: `20`
- Volume Multiplier: `1.5`
- SL ATR Multiplier: `1.5`
- TP ATR Multiplier: `2.5`
- Time Stop: `20` bars

## 3) Logika wejścia
### Long
- EMA(50) > EMA(200)
- Close wybija najwyższy High z ostatnich 20 świec
- ATR(14) > SMA(ATR,100)
- Volume świecy > 1.5 × SMA(Volume,20)

### Short
- EMA(50) < EMA(200)
- Close wybija najniższy Low z ostatnich 20 świec
- ATR i Volume filter jak wyżej

## 4) Zarządzanie pozycją
- Zlecenie Market
- Stop Loss: `ATR * SL ATR Multiplier`
- Take Profit: `ATR * TP ATR Multiplier`
- Time stop: zamknięcie pozycji po zadanej liczbie świec, jeśli nadal otwarta.

## 5) Wskazówki wdrożeniowe
- Startuj na paper/demo.
- Dodaj filtry sesji i maksymalny dzienny drawdown przed live.
- Uwzględnij fee i slippage w testach.
