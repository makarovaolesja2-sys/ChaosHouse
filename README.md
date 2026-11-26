# ChaosHouse Vol.3

Kaosemaja WPF-rakenduse CPU-arvutuse demo. Tegelaste (külmkapp, kummitus, leiutaja) valitud andmetest tehakse käivituse alguses muutumatu hetktõmmis ning arvutatakse iga objekti deterministlik kontrollsumma.

## Käivitamine

Eeldused: Windows, Visual Studio 2022 koos **.NET desktop development** töökoormusega ja .NET 10 SDK.

1. Ava `ChaosHouse.slnx` Visual Studios.
2. Sea `ChaosHouse.Wpf` käivitusprojektiks.
3. Käivita `F5`-ga. Või käivita hoidla juurkaustast `dotnet run --project ChaosHouse.Wpf\ChaosHouse.Wpf.csproj`.

## Vol.3 arvutuse demo

1. Arvutuse tabelis on vaikimisi valitud kolm algset maailma objekti. Vali vähemalt kolm; soovi korral lisa enne uusi elanikke.
2. Jäta töömahuks `20 000 000` tsüklit objekti kohta. Käivituse alguses fikseeritakse ID, nimi, tüüp, energia, vea demo valik ja töömaht hetktõmmiseks.
3. Vali **Järjestikune**, käivita töö ja oota raportit. Iga objekt käivitatakse alles pärast eelmise lõppu.
4. Muutmata sisendiga vali **Paralleelne** ja käivita uuesti. Arvutused käivitatakse `Task.Run`-iga ning oodatakse `Task.WhenAll`-iga. Raport ütleb, kas tulemused kattuvad; mõlema jooksu raport näitab objekti kestust ja kogu töö aega (`Stopwatch`).
5. Tühistuse näitamiseks määra töömahuks näiteks `50 000 000`, käivita arvutus ja vajuta töö ajal **Tühista**. Ühine `CancellationToken` jõuab kõigile arvutustele ja arvutussilm kontrollib seda korduvalt.
6. Vea näitamiseks märgi ühe objekti real **Demoviga** ja käivita vähemalt kolme valitud objektiga. Selle objekti veateade läheb raportisse ning teised objektid lõpetavad oma arvutuse. Järjestikuses režiimis käivituvad need samuti edasi.

Arvutustöö tehakse taustal, nii et WPF-i aken jääb töö ajal reageerivaks. Raportis on iga objekti nimi, lõppolek, tulemus või veateade ja kestus ning jooksu kogukestus. Võrdlus kehtib ainult siis, kui teise režiimi käivitamisel jäävad valitud objektid, vea demo märgid ja töömaht samaks.


# ChaosHouse Vol.4
# ChaosHouse – maailma salvestamine
Kaosemaja WPF-rakenduse maailma salvestamise ja taastamise funktsionaalsus. Rakendus võimaldab salvestada kogu maailma JSON- ja XML-failina või SQL Server LocalDB andmebaasi ning hiljem andmed taastada.

## Salvestamise võimalused
Rakendus toetab kolme salvestusviisi:
JSON – maailma andmed salvestatakse JSON-faili.
XML – maailma andmed salvestatakse XML-faili.
LocalDB – maailma andmed salvestatakse SQL Server LocalDB andmebaasi ChaosHouseDb.

Salvestatakse tegelaste ID-d, nimed, konkreetsed tüübid, energia, tüübipõhised omadused ning gruppide andmed ja gruppidesse kuuluvad tegelased.

## JSON- ja XML-failide kasutamine
Käivita rakendus.
Maailma salvestamiseks vajuta Salvesta maailm ja vali faili asukoht ning vorming (.json või .xml).
Maailma taastamiseks vajuta Laadi maailm ja vali varem salvestatud fail.
Rakendus taastab tegelased, nende omadused ja grupikuuluvused.

## SQL Server LocalDB kasutamine
Eeldused: Windows, SQL Server Express LocalDB ja projektis kasutatav Microsoft.Data.SqlClient pakett.
Andmebaasi nimi: ChaosHouseDb.
Kasutatavad tabelid:
HouseMember – tegelaste andmed;
HouseGroup – gruppide andmed;
GroupMember – gruppide ja tegelaste vahelised seosed.
Ühenduse seadistus asub klassis LocalDbWorldStorage:
Server=(localdb)\MSSQLLocalDB;Database=ChaosHouseDb;Integrated Security=true;
Loo SQL Server LocalDB-s andmebaas ChaosHouseDb ja vajalikud tabelid.
Käivita rakendus.
Maailma salvestamiseks vajuta Salvesta LocalDB.
Maailma taastamiseks vajuta Laadi LocalDB.
Andmete salvestamisel kasutatakse SQL-parameetreid ja tehingut, et tagada andmete terviklikkus.

## Andmete taastamise ja kontrollimise demo
Lisa maailma mitu erinevat tegelast ja loo vähemalt üks grupp.
Salvesta maailm JSON-, XML- või LocalDB-vormingus.
Sulge rakendus ja käivita see uuesti.
Laadi salvestatud maailm ning kontrolli, kas tegelaste ID-d, tüübid, omadused ja grupikuuluvused on säilinud.
Kontrolli tegelaste tegevusi ja reaktsioone pärast laadimist, näiteks käivita tegelase tegevus ning veendu, et energia ja sündmuste reaktsioonid töötavad.

## Vigaste failide kontrollimine
Rakendus kontrollib laaditud andmeid enne aktiivse maailma asendamist.
Loo vigase sisuga JSON- või XML-fail.
Proovi see rakenduses laadida.
Rakendus kuvab veateate ning vigast faili ei laadita.
Olemasolev maailm jääb muutmata.
Laadimisel luuakse kõigepealt taastatud tegelased, hoidlad ja grupid. Aktiivne maailm asendatakse alles pärast andmete edukat lugemist ja kontrollimist.

## Tehniline lahendus
Salvestamise jaoks kasutatakse IWorldStorage liidest ja eraldi salvestusklasse JsonWorldStorage, XmlWorldStorage ning LocalDbWorldStorage. MultiFormatWorldStorage valib salvestusviisi faililaiendi järgi.
Andmete teisendamiseks kasutatakse DTO-objekte (WorldDto, HouseMemberDto, GroupDto), et eraldada salvestatavad andmed rakenduse kasutajaliidese objektidest.
Pärast maailma taastamist luuakse tegelased nende konkreetsete tüüpide järgi ning taastatakse vajalikud sündmuste tellimused, et rakenduse käitumine jätkuks tavapäraselt.