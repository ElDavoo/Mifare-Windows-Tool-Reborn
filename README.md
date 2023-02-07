# Mifare Windows Tool Reborn - MWTR

## Important

As xavave suddenly deleted the repository, the project is now on life support. Maintainer needed!  

Original readme below.

# Mifare Windows Tool - MWT 
 ![MWT_small](https://user-images.githubusercontent.com/3501675/73345370-3cb78d80-4284-11ea-8c17-e67fa8b54adc.png)
 
### [EN] This is a Windows NFC-App for reading, writing, analyzing, cloning, etc. MIFARE® Classic RFID-Tags

### [FR] Application NFC Windows NFC pour lire, écrire, analyser, dupliquer, etc... des badges RFID MIFARE® Classic

#### License

source code is not a copy of original android version, but user interface is strongly inspired from it : I've modified logo and copied icons

It's normal to give credits to the original software author:

The Android application was originally developed on Android systems by Gerhard Klostermeier in cooperation with SySS GmbH (www.syss.de) and Aalen University (www.htw-aalen.de) in 2012/2013. It is free software

Icons used in this application:

original android Logo: Beneke Traub
(Creative Commons 4.0)
Oxygen Icons: www.oxygen-icons.org
(GNU Lesser General Public License)
RFID Tag: www.nfc-tag.de
(Creative Commons 3.0)
MIFARE® is a registered trademark of NXP Semiconductors.

#### GENERAL INFORMATION / INFOS GENERALES

This app was originally made on Android by ikarus23. Please check it out!
https://github.com/ikarus23/MifareClassicTool
You can also donate to them as a thank you.
[Donate with Paypal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=24ET8A36XLMNW) [![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=24ET8A36XLMNW)

[EN] This tool provides several features to interact with MIFARE Classic RFID-Tags with ACR122U tag reader.


It is designed for users who have at least basic familiarity with the MIFARE Classic technology.

More information in <a href="https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/wiki">WIKI<a/> 
 
[FR] Cette outil propose différentes fonctionnalités pour interagir avec les badges RFID MIFARE Classic, à l'aide d'un lecteur de badges ACR-122U.

Il est conçu pour des utilisateurs aillant au moins un minimum de connaissances sur cette technologie.

Plus d'infos sur le wiki ici : https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/wiki/Home-fr-FR

#### PREREQUISITES / PREREQUIS

- Windows 64bits
- [EN] ACR122U Tag Reader / [FR] Lecteur de badges ACR122U --> [AMAZON 34€](https://www.amazon.fr/Luxtech-ACR122U-Lecteur-Contactless-Reader/dp/B078YXN3PH/ref=sr_1_4?__mk_fr_FR=%C3%85M%C3%85%C5%BD%C3%95%C3%91&keywords=acr122U&qid=1580658444&s=kitchen&sr=1-4-catcorr) (ou bien, en vente aussi sur aliexpress, moins cher, mais plus long à faire livrer)

- #### [EN] You need to install these drivers (Native + LibusbK) : documentation here : http://legacy.averbouch.biz/libnfc-and-nfc-utils-binaries-on-windows-10/#howtouse

- #### [FR] Vous devez installer ces pilotes : celui du ACR122U d'origine et aussi LibUsbK : documentation en français ici --> http://legacy.averbouch.biz/fr/libnfc-and-nfc-utils-binaries-on-windows-10/#howtouse 

#### [EN] DOWNLOAD [FR] TELECHARGEMENT / INSTALLATION

[EN] Windows x64 : With MSI Setup provided here : https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/releases

[![image](https://user-images.githubusercontent.com/3501675/73595182-df3a6f80-4515-11ea-915a-011c9f363317.png)](https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/releases)


[FR] Installation sur Windows 64 bits avec le fichier de setup fourni ici : https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/releases

#### LANGUAGES / LANGUES

![image](https://user-images.githubusercontent.com/3501675/73600638-85589a80-4553-11ea-9637-1ede0cd8856c.png)

[EN] This tool is natively in english but, a french translation exists (automatic loading of french locale if your windows is in french)

[FR] Cet outil se lancera automatiquement en français si votre windows est en français

![image](https://user-images.githubusercontent.com/3501675/73377680-b0c35700-42bf-11ea-8002-4fda409fd045.png)


#### IMPORTANT NOTES / REMARQUES IMPORTANTES

Some important things are:

- The features this tool provides are very basic.

- The first block of the first sector of an original
MIFARE Classic tag is read-only i.e. not writable. But there
are special MIFARE Classic tags that support writing to the
manufacturer block with a simple write command. This App is able to
write to such tags and can therefore create fully correct clones.

- However, some special tags require a special command sequence
to put them into the state where writing to the manufacturer block is possible.
These tags will not work.

- Remember this when you are shopping for special tags!

#### [EN] IF YOU ENCOUNTER A PROBLEM PLEASE REPORT IT <a href="https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/issues">HERE</a> 

#### [FR] SI VOUS RENCONTREZ DES PROBLEMES D'INSTALLATION ET/OU D'UTILISATION, SIGNALEZ LES <a href="https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/issues">ICI</a>  :

| <img src="https://user-images.githubusercontent.com/3501675/73281623-8c4c7980-41f0-11ea-967b-f649b0147f0a.png" width="300" height="auto" /> | <img src="https://user-images.githubusercontent.com/3501675/73309783-3f35cb00-4223-11ea-9df6-73375f301b28.png" width="600" height="auto" /> |
|---|---|
| ![image](https://user-images.githubusercontent.com/3501675/73364730-57e9c380-42ab-11ea-8a4c-31f3b0dace5c.png) |  ![image](https://user-images.githubusercontent.com/3501675/73280408-c3219000-41ee-11ea-8e17-c7e6b5b952b8.png) |
|---|---|
| <img src="https://user-images.githubusercontent.com/3501675/73311455-b91b8380-4226-11ea-8ff7-c53153d2ab51.png" width="600" height="auto" /> | <img src="https://user-images.githubusercontent.com/3501675/73455790-073c9e00-4371-11ea-8d52-8b9b7bde8c3d.png" width="500" height="auto" /> |  
|---|---|

update january 2022: updated libusbK 3.1.0.0 installation guide: https://github.com/ElDavoo/Mifare-Windows-Tool-Reborn/issues/27#issuecomment-1013957721
