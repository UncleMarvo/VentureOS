Domain

• Technology independent
• Static Create()
• Static Restore()
• Aggregate owns mutations

Application

• One handler per use case
• Result<T>

Infrastructure

• Repository orchestrates persistence
• One Store per child collection
• Value Object ↔ primitive conversion only here

API

• Endpoint extension per feature
• Request DTOs live with endpoint