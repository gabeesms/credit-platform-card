# ADR-0002 — Consuming approved credit events

## Status

Accepted (local demonstration)

## Context

Credit publishes versioned `CreditApproved` events through the shared durable topic
exchange `credit.events`, using the routing key `credit.approved`. RabbitMQ may
deliver an event more than once, and Card must not read Credit's database.

## Decision

Card declares its own durable queue `card.credit-approved` and binds it only to
`credit.approved`. It declares and binds a durable dead-letter queue for invalid
or failed deliveries. It does not use or alter the manually created inspection
queue.

For a valid version 1 `CreditApproved` event, Card creates one card with an
internal id, the customer id, the credit application id, a `StandardCredit` type,
the approved amount as its simulated limit, and `Active` status. The consumer
persists the card and an inbox record keyed by `eventId` in one database save
before acknowledging the RabbitMQ message. A unique constraint on
`creditApplicationId` prevents a second card for the same application even if a
redelivery has another event id.

Invalid payloads and handler failures are logged and dead-lettered rather than
requeued indefinitely. The dead-letter queue is available for operator inspection
and manual recovery.

## Alternatives considered

- Query the Credit database after receiving a notification.
- Acknowledge before persisting the card.
- Process every delivery without an inbox or uniqueness guard.
- Send poison messages straight back to the queue indefinitely.

The selected event-driven design keeps service ownership separate, persists before
acknowledging, is idempotent, and gives failed deliveries a place for inspection.

## Consequences

- Card does not learn about an approval published before its queue binding existed;
  RabbitMQ does not replay messages to newly declared queues.
- The consumer provides at-least-once delivery with idempotent persistence, not
  exactly-once message transport.
- Dead-letter messages require inspection and explicit recovery.
