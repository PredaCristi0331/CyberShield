„CyberShield: Platformă desktop inteligentă pentru analiza și detecția conținutului video Deepfake folosind modele de Machine Learning”

2. Caracteristicile principale ale proiectului (Punctele tale forte)

Securitate și Confidențialitate (Offline-First): Inferența (analiza AI) se face 100% local, direct pe mașina utilizatorului, fără a trimite videoclipuri sensibile în cloud, respectând politicile de confidențialitate (GDPR).

Arhitectură Software Modernă (Clean Architecture): Proiectul nu este un cod „aruncat” la un loc. Este divizat riguros în straturi (UI, Presentation, Application, Domain, Infrastructure), permițând scalabilitatea, testarea ușoară și înlocuirea componentelor (ex: trecerea de la SQLite la SQL Server fără a schimba logica aplicației).

Pipeline de Procesare End-to-End optimizat: Aplicația gestionează fluxul complet: extragerea asincronă a cadrelor (decodare hardware), preprocesare, inferență în batch-uri (grupuri de cadre pentru performanță) și agregarea scorului final.

Sistem de Caching Inteligent: Folosește algoritmi de hashing (SHA-256). Dacă un videoclip a fost deja scanat în trecut, aplicația recunoaște fișierul și returnează instant rezultatul din baza de date locală, economisind resurse de procesare.

Interfață Utilizator (UX/UI) de ultimă generație: Design modern (Fluent Design), complet asincron (UI-ul nu se blochează niciodată în timpul procesării grele), suport pentru Drag & Drop și vizualizare dinamică a riscului direct peste playback-ul video.

Generare Automată de Rapoarte (Audit): La finalul scanării, sistemul generează instant un raport PDF profesional, util pentru audit de securitate, investigații criminalistice (forensics) sau arhivare.


3. Tehnologii, Limbaje și Framework-uri Utilizate (Tech Stack)

Limbaj de programare central:

C# (pe platforma .NET 8 sau .NET 9)
Interfață Grafică (Frontend):

WPF (Windows Presentation Foundation) – Pentru randarea interfeței.
XAML – Limbajul de markup pentru design.
CommunityToolkit.Mvvm – Pentru implementarea curată a șablonului de design MVVM (Model-View-ViewModel) și Data Binding.
Inteligenta Artificială & Machine Learning:

ML.NET (Microsoft.ML) – Framework-ul principal pentru orchestrarea fluxului de ML.
ONNX Runtime (Microsoft.ML.OnnxTransformer) – Utilizat pentru încărcarea și rularea eficientă a modelului AI pre-antrenat (format .onnx) capabil să detecteze manipulările faciale/video.
Procesare Video & Media:

FFmpeg.Autogen – Wrapper C# peste librăriile native FFmpeg, utilizat pentru decodarea de înaltă performanță a fișierelor video (MP4, MKV) la nivel de cadru (frame).
Baze de Date & Persistență:

SQLite – Bază de date relațională, ușoară, integrată local.
Entity Framework Core (EF Core) – ORM (Object-Relational Mapper) folosit pentru lucrul cu baza de date folosind obiecte C#, migrații și interogări LINQ.
Arhitectură, Raportare și Testare:

QuestPDF – Librărie modernă de înaltă performanță pentru generarea rapoartelor PDF de audit.
Dependency Injection (DI) – Utilizat prin Microsoft.Extensions.DependencyInjection pentru a decupla componentele (bune practici SOLID).
xUnit & Moq – Pentru implementarea testelor unitare și de integrare, validând corectitudinea Use Case-urilor.
