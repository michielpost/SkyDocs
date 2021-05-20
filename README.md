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

Try the app hosted on Sia Skynet (first load can be slow, after that it's fast): https://skydocs.hns.siasky.net

---

## Screenshots
TODO: New screenshots have to be made

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

## SkyDB Debut Hackathon
SkyDocs was created for *[The SkyDB Debut](https://gitcoin.co/hackathon/skydb/)* hackathon. 
Finished as Runner Up in the SkyDB Debut Hackathon: https://blog.sia.tech/skydb-debut-hackathon-winners-d7929970cdea  

Jury notes:
![Hackathon results](screenshots/hackathon_winner.jpg)

## Original screenshots
Demo video: https://siasky.net/AADh9ZruJIm-UqRKWNCA_uCh1sYYg4NvrWZUKKw3UbwXiw

Login
![Login](screenshots/01_login.png)

View all your documents
![View all your documents](screenshots/02_documents.png)

Edit a document
![Edit a document](screenshots/03_edit_document.png)