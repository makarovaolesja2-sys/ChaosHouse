# Kaosemaja – Chaos House

## 1. Projekti eesmärk

Kaosemaja on humoorikas WPF-rakendus, kus erinevad tegelased elavad ühes majas ja käituvad erinevalt. Projekti eesmärk on harjutada objektorienteeritud programmeerimist C# keeles, sealhulgas pärilust, liideseid, polümorfismi, valideerimist ja kollektsioonide kasutamist.

## 2. Tehnoloogiad

- C#
- .NET 10
- WPF
- `ObservableCollection<T>`
- `INotifyPropertyChanged`
- Git ja GitHub

## 3. Projekti struktuur

- `ChaosHouse.Core` – sisaldab baasklassi, liideseid, alamklasse, olekut ja äriloogikat.
- `ChaosHouse.Wpf` – sisaldab kasutajaliidest, nuppe, andmesidumist ja sündmuste logi.

WPF-projekt kasutab Core-projekti. Core ei sõltu WPF-ist.

## 4. Klassihierarhia ja liidesed

Abstraktne baasklass `HouseMember` sisaldab elaniku ID-d, nime, tüüpi ja energiataset. Kõik elanikud alustavad 100% energiaga.

Alamklassid:

- `TalkingFridge` – rääkimisvõimeline külmkapp, mis saab köögi lukustada ja avada.
- `Ghost` – kummitus, kelle hirmutase on muudetav.
- `Inventor` – leiutaja, kes saab kirjeldatud sisendi põhjal uue vidina välja mõelda.

Liidesed:

- `ICook` – kirjeldab toidu valmistamise tegevust.
- `IScare` – kirjeldab hirmutamise tegevust.
- `IRepair` – kirjeldab parandamise tegevust.

`TalkingFridge` rakendab liideseid `ICook` ja `IRepair`. `Ghost` rakendab `IScare` ning `Inventor` rakendab `IRepair`.

## 5. CrazyAction ja olekureeglid

Kõik elanikud pakuvad meetodit `CrazyAction()`, kuid iga alamklass käitub erinevalt.

- Külmkapi käsk `lock` lukustab köögi ja vähendab energiat 15 ühiku võrra.
- Külmkapi käsk `unlock` avab luku.
- Kummituse sisend peab olema täisarv vahemikus 0–100. Edukas tegevus vähendab energiat 10 ühiku võrra.
- Leiutaja vajab mittetühja vidina nime. Edukas tegevus vähendab energiat 25 ühiku võrra.

Energia peab jääma vahemikku 0–100. Energiat muudetakse Core-projektis ning `INotifyPropertyChanged` võimaldab WPF-il muudatusi automaatselt kuvada. Vigane sisend ei tohi muuta vastavat olekut.

## 6. Käivitamine

1. Klooni GitHubi repositoorium.
2. Ava `ChaosHouse` lahendus Visual Studios.
3. Veendu, et `ChaosHouse.Wpf` kasutab projekti `ChaosHouse.Core`.
4. Vali `ChaosHouse.Wpf` käivitusprojektiks.
5. Koosta lahendus käsuga **Build → Rebuild Solution**.
6. Käivita rakendus klahviga `F5`.

## 7. Kontrollitud kasutusjuhud

Järgmised juhud tuleb enne esitamist rakenduses läbi proovida ja tulemused kinnitada.

1. Rakendus käivitub ning kuvab kolm algset elanikku.
2. Külmkapi käsk `lock` vähendab energiat 100%-lt 85%-le.
3. Külmkapi korduv `lock` vähendab energiat 85%-lt 70%-le.
4. Külmkapi käsk `unlock` avab luku.
5. Kummituse sisend `80` muudab hirmutaseme 80%-ks ja vähendab energiat.
6. Kummituse sisend `150` lükatakse tagasi ning hirmutase ei muutu.
7. Leiutaja loob sisestatud nimega vidina ja energia väheneb.
8. Tühja sisendi korral ei muudeta objekti olekut.
9. Elaniku lisamine ja eemaldamine toimivad ning sündmused ilmuvad logisse.
10. Valitud elaniku energiataset kuvatakse `ProgressBar`-is.

## 8. Iseseisvalt õpitud WPF-element

Kasutan `ProgressBar`-i elaniku energiataseme kuvamiseks. See muudab objekti oleku kasutajale arusaadavaks ning aitab näha, kuidas `INotifyPropertyChanged` ja andmesidumine võimaldavad kasutajaliidest automaatselt uuendada.

Lisaks kasutan `DataTemplate`-it, et kuvada iga elaniku tüüpi ja nime ühtses loendivaates.

## 9. GitHubi koostöö

- Kaasüliõpilane: [nimi]
- Issue: https://github.com/makarovaolesja2-sys/ChaosHouse/issues/1
- Pull Request: [Pull Request URL]
- Ühendamine `main`-haruga: [kuupäev ja tulemus]

Kaasüliõpilase muudatus lisatakse eraldi harus ning kontrollitakse enne ühendamist `main`-haruga. Pull Request seotakse vastava Issue'ga.

## 10. AI kasutamine

Kasutasin OpenAI ChatGPT-d C#- ja WPF-koodi mõistmiseks, eestikeelsete kasutajaliidese tekstide koostamiseks ning vigade leidmise ja parandamise juhiste saamiseks.

Kontrollisin rakenduse käivitamist, energia muutumist, sisendi valideerimist ja kasutajaliidese toimimist Visual Studios. Vastutan ise lõpliku koodi, testimise ja esitamise eest ning pean suutma selgitada kõiki lahenduse osi.

## 11. Lõplik kontroll

Enne esitamist kloonin repositooriumi uude kausta, avan lahenduse kloonitud koopiast ning kontrollin, et see kompileerub ja käivitub ilma algse arenduskeskkonna lisaseadistusteta.
