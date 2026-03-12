# Quantower - Altcoin M1 Momentum Breakout

## 0) Czy trzeba Visual Studio?
**Nie, nie musisz używać Visual Studio.**  
Do uruchomienia tej strategii wystarczy sam **Quantower + moduł Algo**.

Masz 2 opcje:
1. **Tylko Quantower Algo (najprościej)** – tworzysz nową strategię w Algo, wklejasz kod i klikasz Build/Run.
2. **Visual Studio (opcjonalnie)** – przydatne, jeśli chcesz wygodniejszą edycję/debug i większy projekt.

Jeśli dopiero zaczynasz, wybierz opcję nr 1 (bez Visual Studio).

---

Poniżej masz dokładnie **jak przygotować strategię do aplikacji Quantower i ją uruchomić**.

## 1) Wymagania
- Zainstalowany **Quantower** (desktop).
- Aktywne połączenie do brokera/giełdy w Quantower (np. Binance/Bybit itp.).
- Włączony moduł **Algo** w Quantower.

> Ten repo zawiera gotowy plik strategii: `AltcoinM1MomentumBreakoutStrategy.cs`.

---

## 2) Jak dodać plik strategii do Quantower (najprościej)
1. Otwórz **Quantower**.
2. Przejdź do modułu **Algo**.
3. Utwórz nową strategię C# (np. `New Strategy`).
4. W edytorze projektu podmień zawartość klasy na kod z pliku:
   - `Quantower/AltcoinM1MomentumBreakoutStrategy.cs`
5. Kliknij **Build/Compile**.
6. Upewnij się, że strategia pojawiła się na liście jako:
   - `Altcoin M1 Momentum Breakout`.

Jeśli kompilacja przejdzie, strategia jest gotowa do uruchomienia.

---

## 2A) Jeśli kompilujesz w Visual Studio – gdzie skopiować plik?
Po buildzie z Visual Studio potrzebujesz skopiować **DLL strategii** do folderu, który Quantower skanuje dla Algo.

### Najpewniejsza metoda (polecana)
1. W Quantower wejdź w **Algo**.
2. Użyj opcji typu **Open local folder / Open Algo folder** (nazwa może się różnić zależnie od wersji).
3. Otworzy się katalog użytkownika Quantower.
4. Wejdź do podfolderu strategii (najczęściej `Algo/Strategies`) i tam skopiuj skompilowaną DLL.
5. Wróć do Quantower i użyj **Reload/Refresh** w module Algo.

### Typowa ścieżka w Windows (przykład)
- `C:\Users\<TwojUser>\Documents\Quantower\Algo\Strategies\`

### Co kopiujesz z Visual Studio
- Plik z builda, np.:
  - `bin\Debug\<target>\TwojaStrategia.dll`
  - albo `bin\Release\<target>\TwojaStrategia.dll`

> Jeśli nie widzisz strategii po skopiowaniu DLL, sprawdź logi Algo i zgodność target framework z wersją Quantower.

---

## 3) Jak uruchomić strategię na wykresie
1. Otwórz wykres wybranego altcoina i ustaw interwał **1m**.
2. Dodaj/uruchom strategię **Altcoin M1 Momentum Breakout**.
3. W parametrach strategii ustaw:
   - **Account** (konto demo/live),
   - **Symbol** (instrument),
   - **Quantity** (wielkość pozycji),
   - resztę parametrów (poniżej wartości startowe).
4. Kliknij **Run/Start**.
5. Sprawdź logi strategii (czy nie ma błędów typu brak konta/symbolu).

---

## 4) Parametry startowe (bezpieczny punkt wyjścia)
- Fast EMA: `50`
- Slow EMA: `200`
- Breakout Lookback: `20`
- ATR Period: `14`
- ATR SMA Period: `100`
- Volume SMA Period: `20`
- Volume Multiplier: `1.5`
- SL ATR Multiplier: `1.5`
- TP ATR Multiplier: `2.5`
- Enable Time Stop: `true`
- Time Stop (bars): `20`
- Allow Long: `true`
- Allow Short: `true`

---

## 5) Co strategia robi (skrót)
### Wejście Long
- EMA(50) > EMA(200)
- Zamknięcie świecy wybija najwyższy High z ostatnich 20 świec
- ATR(14) > SMA(ATR,100)
- Volume świecy > 1.5 × SMA(Volume,20)

### Wejście Short
- EMA(50) < EMA(200)
- Zamknięcie świecy wybija najniższy Low z ostatnich 20 świec
- Filtry ATR i Volume jak wyżej

### Wyjście
- Market entry + SL/TP liczone z ATR
- Opcjonalny **time stop** (zamknięcie po `Time Stop (bars)`)

---

## 6) Najczęstsze problemy i szybkie rozwiązania
- **"Account not selected"** w logu:
  - wybierz konto w ustawieniach strategii.
- Brak transakcji:
  - sprawdź, czy wykres jest na **1m**,
  - tymczasowo obniż `Volume Multiplier` (np. 1.2),
  - sprawdź płynność instrumentu.
- Za dużo wejść:
  - zwiększ `Breakout Lookback` (np. 30),
  - zwiększ `Volume Multiplier` (np. 1.8).

---

## 7) Rekomendowane wdrożenie
1. Najpierw odpal na **demo/paper** minimum 1–2 tygodnie.
2. Porównaj wyniki z backtestem (uwzględnij fee i slippage).
3. Dopiero potem uruchom na małym kapitale live.

Powodzenia — to jest gotowy szablon, który możesz dalej stroić pod konkretne altcoiny.
