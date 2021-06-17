# SkyDocs
[![Build .NET](https://github.com/michielpost/SkyDocs/actions/workflows/build.yml/badge.svg)](https://github.com/michielpost/SkyDocs/actions/workflows/build.yml)

A Sia Skynet Google Docs alternative.

**Live version**: https://skydocs.hns.siasky.net

Features:
- Login with your personal username and password
- See a list of your documents
- Screenshot preview of all documents
- Edit documents
- Delete documents
- Data is encrypted (except preview images)

SkyDocs is build with Blazor and runs on WebAssembly. The [SiaSkynet C# SDK](https://github.com/michielpost/SiaSkynet) is used to communicate with Skynet and SkyDB.

Try the app hosted on Sia Skynet: https://skydocs.hns.siasky.net

---
## Instal for local development
- Install Visual Studio Code https://code.visualstudio.com/
- Install .Net 5 SDK https://dotnet.microsoft.com/download/dotnet/5.0
- Compile and run the project

## Open Source Project Credits
- [SiaSkynet C# SDK](https://github.com/michielpost/SiaSkynet)
- [MetaMask.Blazor](https://github.com/michielpost/MetaMask.Blazor)
- [Radzen.Blazor UI Components](https://github.com/radzenhq/radzen-blazor)
- [Blazored.LocalStorage](https://github.com/blazored/LocalStorage)
- [Toolbelt.Blazor.HeadElement](https://github.com/jsakamoto/Toolbelt.Blazor.HeadElement)

## Acknowledgements
Development of SkyDocs v2 has been made possible with a grant from [The Graph](https://thegraph.com/blog/wave-one-funding).

## Screenshots
Login
![Login](screenshots/v2_01_login.PNG)

View all your documents
![View all your documents](screenshots/v2_02_documents.PNG)

Edit a document
![Edit a document](screenshots/v2_03_edit_document.png)

Share a document
![Edit a document](screenshots/v2_04_share.png)

View documents shared with you
![Edit a document](screenshots/v2_05_share_list.png)

## SkyDB Debut Hackathon
SkyDocs was created for *[The SkyDB Debut](https://gitcoin.co/hackathon/skydb/)* hackathon. 
Finished as Runner Up in the SkyDB Debut Hackathon: https://blog.sia.tech/skydb-debut-hackathon-winners-d7929970cdea  

[More info and screenshots can be found here.](SkyDocsV1.md)