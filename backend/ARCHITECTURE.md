# IPI Pro — Architecture & Design

## System Overview

### Multi-Tenant Architecture
- **Isolation Boundary**: Lab (tenant)
- **Tenant Context**: HTTP header `X-Lab-Id`
- **Enforcement**: Query filters + explicit checks

### Data Flow
