# DynaChange&trade; Business Rule Examples

![DynaChange&trade; Rules](../lib/assets/icon-dyna-rules.png)

This project contains example Rule code for extending Prophet 21 via DynaChange&trade; Business Rules.

## General

### `ValidDatetime.cs`

This is a single-row Rule that will validate that each field passed in has a proper date format.

### `ValidUrl.cs`

This is a single-row Rule that will validate that each field passed in has a proper URL format.

## OnDemand

### `AddNewRowForDiscount.cs`

Adds a new line item (other charge item) to the order to provide a flat percentage discount based on the order total.

## OnEvent - FormDatastream

### `FormDatastream_AddGroup.cs`

This Rule is fired when the Form Datastream is created for invoices. It simply adds a group to the header and line levels in the datastream file with hardcoded values. Typically, one would add a group when you have detail information for the header or line that could have multiple records per header or line.

### `FormDatastream_SortInvoiceLines.cs`

This Rule is fired when the Form Datastream is created for invoices. It will look up a user defined field for each item on the invoice and add it to the Datastream so that information can be printed on the form. It will then also sort the invoice lines based on this new field.

## OnEvent - MessageBox

### `SuppressExpediteDateMessage.cs`

Simple rule to show how to change the option that is being selected for a message-box in P21, and to have that message suppressed.

## OnEvent - OrderUpdated

### `OrderUpdatedEvent.cs`

This is a simple Rule to show how the Order Updated event-based Rule can be implemented. When an OnEvent type Rule is set up in P21 for the `OrderUpdated` event, this Rule will be fired. The Rule simply drops a text file in `C:\temp` containing the order number that was updated, and the action (_ADD_ if the order was just created, or _UPDATE_ if the order was modified).

## Validator

### `OrderCreditCheck.cs`

Checks order total by summing extended prices for all order lines.

- If >= $1000, sets all lines' `allocated_qty` to 0 and `disposition` to "H"
- If < $1000, sets all lines' `allocated_qty` to match the `unit_quantity`

### `OrderLineCreditCheck.cs`

Checks an order line's extended price.

- If >= $1000, sets the line's `allocated_qty` to 0 and `disposition` to "H"
- If < $1000 sets the line's `allocated_qty` to match the `unit_quantity`
