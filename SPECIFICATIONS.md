# TSON Binary Format Specification

This document outlines the binary format for serializing and deserializing JSON-like data structures into a compact, typed, and compressed binary format. The specification includes predefined types, custom types, and the structure of a serialized file.

## Version 1.0

### 1. Predefined Types

The format supports several predefined types, each identified by a unique ID and occupying a specific amount of space in the binary representation.

* Boolean (ID: 1)
  Size: 1 byte
  Values: `0x00` for `false`, `0x01` for `true`

* Int32 (ID: 2)
  Size: 4 bytes (32-bit signed integer)
  Stored in little-endian format.

* Double (ID: 3)
  Size: 8 bytes (64-bit floating-point number)
  Stored in little-endian format.

* String (ID: 4)
  Format: `<uint8*><\0>`
  A UTF-8 encoded string terminated by a null byte (`\0`). The length is not explicitly defined, as it is inferred from the presence of the null terminator.

* Array (ID: 5)
  Format: `<items_count: uint32><items*>`
  * `items_count`: The number of elements in the array.
  * `items*`: A continuous block of `items_count` elements, all of the specified type.

* Object (ID: 6)
  Format: `<items_count: uint32><(key: string, value: item)*>`
  * `items_count`: The number of key-value pairs in the object.
  * `(key: string, type: uint32, value: item)*`: A sequence of `items_count` key-value pairs, where each key is a UTF-8 string.

### 2. Custom Type Description (ID: 7 + pos)

Custom types are defined with a list of properties, where each property has a name and a corresponding type.

* Properties:
  The properties section defines the structure of the type, ending when the current byte equals 0x00. Each property includes:
  * `property_name`: A UTF-8 encoded string terminated by a null byte (\0).
  * `property_type_id`: A uint32 that identifies the type of the property (predefined or custom).

> NOTE: If the `property_type_id` is of type `5` (array), an other ID follow this to specify the type of items (same for root item)

### 3. Item Content for Custom Types

When an item is an instance of a custom class type, it is structured as follows:

* Header:
  *Only on nullable types (string, array, object and custom types)*
  * `is_null`: Indicates if the value is `null` (1 byte)

* Values:
  A list of property values in the order they were defined in the type description.

### 4. File Structure

The binary format consists of a header, a type definition section, and the actual content of the document.

* Header:
  The file begins with a header that identifies the format and provides metadata about the size of various sections.
  * `format_id`: 2 bytes, always 0x54FA (hexadecimal).
  * `format_version`: 1 byte, indicates the version of the binary format (in this case, 1 for version 1.0).
  * `types_size`: 4 bytes (uint32), specifies the size in bytes of the type definitions section.
  * `content_size`: 4 bytes (uint32), specifies the size in bytes of the document content section.

  * Type Definitions:
    The type definition section describes all custom types used in the document.
    * `types_count`: 4 bytes (uint32), indicates how many custom types are defined.
    * `types_list`: A list of type descriptions, as specified in section 2. Each type has an ID starting from 7 and is followed by its property definitions.

* Content Section:
  The content section contains the serialized document's data, starting with the root item.
  * `root_type`: 4 bytes (uint32), the type ID of the root item (which can be predefined or custom).
  * `root_item`: The serialized root item, structured according to its type.

### 5. Endianness

All multi-byte values (e.g., int32, double, uint32) are stored in little-endian (LE) format. This applies to both predefined types and file structure elements.

### 6. Example Layout

A serialized file might look like this:

```
    Header:
        Format ID: 0x54FA
        Version: 0x01
        Type Definition Size: 48 bytes
        Document Content Size: 128 bytes

    Type Definitions:
        Number of Types: 1
            Type 7: Describes a custom object with two properties:
                name (string)
                age (int32)

    Content:
        Root Item Type ID: 5 (Array of custom objects 7)
        Root Item: Serialized array of custom objects.
```