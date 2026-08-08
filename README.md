# IPI Pro — Specimen Check-In

A small, self-contained vertical slice of a real healthcare SaaS feature. Lab technicians receive specimen shipments and check in bottles against an itemized manifest, reconciling mismatches before closing.

---

## Stack

- **Backend**: ASP.NET Core 8 (.NET 8) + Entity Framework Core  
- **Database**: SQLite (lightweight, zero-config for local dev)  
- **Frontend**: Vue.js 3 + Vite + Tailwind CSS  
- **Testing**: xUnit + In-Memory EF Core  
- **Tenant Isolation**: Server-enforced query filters + explicit LabId checks

---

## Quick Start

### Backend
