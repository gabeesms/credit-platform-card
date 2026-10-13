# ADR-0001 — Card data ownership

## Status

Accepted

## Context

Card Service needs to register cards associated with customer and approved credit
application identifiers. Customer and Credit own their respective data and
databases.

## Decision

Card Service owns card records, lifecycle state, schema, migrations, and its
`CardServiceDb` database. It stores external customer and credit application ids
as identifiers only. It does not query or write the Customer or Credit databases
and creates no cross-service foreign keys.

The demonstration stores only internal card ids and simulated card attributes.
It does not create or persist real card numbers, CVVs, expiry data, or payment
credentials.

## Consequences

- Card can manage its schema independently of Customer and Credit.
- A card's association to an approved credit application is enforced within the
  Card database by a unique application id, not by a foreign key to Credit.
- Future payment integrations require a separately reviewed design and must not
  repurpose this demonstration model as a real issuer.
