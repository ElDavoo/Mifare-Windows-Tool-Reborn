# Mifare Windows Tool - MWT 
 ![MWT_small](https://user-images.githubusercontent.com/3501675/73345370-3cb78d80-4284-11ea-8c17-e67fa8b54adc.png)
 
### [EN] This is a Windows NFC-App for reading, writing, analyzing, cloning, etc. MIFARE® Classic RFID-Tags

### [FR] Application NFC Windows NFC pour lire, écrire, analyser, dupliquer, etc... des badges RFID MIFARE® Classic

#### GENERAL INFORMATION / INFOS GENERALES

[EN] This tool provides several features to interact with MIFARE Classic RFID-Tags with ACR122U tag reader.

It is designed for users who have at least basic familiarity with the MIFARE Classic technology.

More information in <a href="https://github.com/xavave/Mifare-Windows-Tool/wiki">WIKI<a/> 
 
[FR] Cette outil propose différentes fonctionnalités pour interagir avec les badges RFID MIFARE Classic, à l'aide d'un lecteur de badges ACR-122U.

Il est conçu pour des utilisateurs aillant au moins un minimum de connaissances sur cette technologie.

Plus d'infos sur le wiki ici : https://github.com/xavave/Mifare-Windows-Tool/wiki/Home-fr-FR

#### PREREQUISITES / PREREQUIS

- Windows 64bits
- [EN] ACR122U Tag Reader / [FR] Lecteur de badges ACR122U
- #### [EN] You need to install these drivers (Native + LibusbK) : documentation here : http://legacy.averbouch.biz/libnfc-and-nfc-utils-binaries-on-windows-10/#howtouse

- #### [FR] Vous devez installer ces pilotes : celui du ACR122U d'origine et aussi LibUsbK : documentation en français ici --> http://legacy.averbouch.biz/fr/libnfc-and-nfc-utils-binaries-on-windows-10/#howtouse 

#### [EN] DOWNLOAD [FR] TELECHARGEMENT / INSTALLATION

[EN] Windows x64 : With MSI Setup provided here : https://github.com/xavave/Mifare-Windows-Tool/releases

![image](https://user-images.githubusercontent.com/3501675/73595182-df3a6f80-4515-11ea-915a-011c9f363317.png)

![image](https://user-images.githubusercontent.com/3501675/73595165-997da700-4515-11ea-8a84-f1d6ff411549.png)

#### Important : Run Mifare Windows Tool as Administrator or it may not be able to write in folder c:\program Files\MWT\dumps and you would see error : "Cannot open: dumps\mfc_....dump, exiting" when reading a tag

[![releases](https://user-images.githubusercontent.com/3501675/73595069-a77ef800-4514-11ea-848a-3a00deaa2b5d.png)](https://github.com/xavave/Mifare-Windows-Tool/releases)

[FR] Installation sur Windows 64 bits avec le fichier de setup fourni ici : https://github.com/xavave/Mifare-Windows-Tool/releases

#### Attention : Il faut lancer Mifare Windows Tool en tant qu'administrateur pour pouvoir l'utiliser pleinement

![image](https://user-images.githubusercontent.com/3501675/73595037-35a6ae80-4514-11ea-9373-8e485c9a64ac.png)

#### LANGUAGES / LANGUES

![image](https://user-images.githubusercontent.com/3501675/73600585-ab316f80-4552-11ea-8cf7-bed29bf679e3.png)

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

#### [EN] IF YOU ENCOUNTER A PROBLEM PLEASE REPORT IT <a href="https://github.com/xavave/Mifare-Windows-Tool/issues">HERE</a> 

#### [FR] SI VOUS RENCONTREZ DES PROBLEMES D'INSTALLATION ET/OU D'UTILISATION, SIGNALEZ LES <a href="https://github.com/xavave/Mifare-Windows-Tool/issues">ICI</a>  :

| <img src="https://user-images.githubusercontent.com/3501675/73281623-8c4c7980-41f0-11ea-967b-f649b0147f0a.png" width="300" height="auto" /> | <img src="https://user-images.githubusercontent.com/3501675/73309783-3f35cb00-4223-11ea-9df6-73375f301b28.png" width="600" height="auto" /> |
|---|---|
| ![image](https://user-images.githubusercontent.com/3501675/73364730-57e9c380-42ab-11ea-8a4c-31f3b0dace5c.png) |  ![image](https://user-images.githubusercontent.com/3501675/73280408-c3219000-41ee-11ea-8e17-c7e6b5b952b8.png) |
|---|---|
| <img src="https://user-images.githubusercontent.com/3501675/73311455-b91b8380-4226-11ea-8ff7-c53153d2ab51.png" width="600" height="auto" /> | <img src="https://user-images.githubusercontent.com/3501675/73455790-073c9e00-4371-11ea-8d52-8b9b7bde8c3d.png" width="500" height="auto" /> |  
|---|---|

