package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.connector.sdk.DynamicEntity;
import com.sdl.tridion.connectorframework.contracts.schema.EntitySchema;
import com.sdl.tridion.connectorframework.contracts.schema.FieldType;
import com.sdl.tridion.connectorframework.contracts.schema.Schema;
import com.sdl.tridion.connectorframework.contracts.schema.SchemaFieldDefinition;
import com.sdl.tridion.remoting.contracts.DynamicValueObject;

import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.*;

import static com.sdl.tridion.connectorframework.connector.sdk.utils.FieldUtils.toCamelCase;

/**
 * Salesforce Utils.
 *
 * Various util functions to convert between entities and Salesforce data, getting field type etc.
 */
public final class SalesforceUtils {

    private SalesforceUtils() {
    }

    static public FieldType getFieldType(String type) {

        // TODO: Add support for nested types
        // TODO: How do we check if for example 'address' is a nested type? Can we get that type as well??


        if (type.equalsIgnoreCase("boolean")) {
            return FieldType.Boolean;
        }
        if (type.equalsIgnoreCase("int")) {
            return FieldType.Integer;
        }
        if (type.equalsIgnoreCase("double")) {
            return FieldType.Float;
        }
        if (type.equalsIgnoreCase("datetime")) {
            return FieldType.DateTimeOffset;
        }
        return FieldType.String;
    }

    static public boolean isListType(String type) {
        if (type.equalsIgnoreCase("multipicklist")) {
            return true;
        }
        return false;
    }

    static public DynamicEntity toEntity(Map map, EntitySchema schema, Map<String,String> nameMappings) {
        DynamicEntity entity = new DynamicEntity();
        for (SchemaFieldDefinition fieldDefinition : schema.getFields()) {
            String targetFieldName = fieldDefinition.getName();
            String sourceFieldName = toSalesforceFieldName(targetFieldName, nameMappings);
            Object fieldValue = map.get(sourceFieldName);
            if (fieldValue == null) {
                // Try to use a field name in pascal case. It needed for custom fields with namespace prefix.
                //
                fieldValue = map.get(toCamelCase(sourceFieldName));
            }
            if (fieldValue != null) {
                if (fieldDefinition.getType() == FieldType.DateTimeOffset) {
                    fieldValue = ZonedDateTime.parse(fieldValue.toString(), DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSZ"));
                } else if (fieldDefinition.getType() == FieldType.String && fieldDefinition.isList()) {
                    // For now only String lists are supported
                    //
                    List<String> list = new ArrayList<>();
                    StringTokenizer tokenizer = new StringTokenizer(fieldValue.toString(), ";");
                    while (tokenizer.hasMoreTokens()) {
                        list.add(tokenizer.nextToken());
                    }
                    fieldValue = list;
                }
                entity.setField(targetFieldName, fieldValue);
            }
            // TODO: Add support for nested types here
        }
        return entity;
    }

    static public Map<String,Object> toMap(DynamicValueObject valueObject, Schema schema, Map<String,String> nameMappings, boolean onlyWritableFields) {
        Map<String,Object> map = new HashMap<>();
        for (SchemaFieldDefinition fieldDefinition : schema.getFields()) {
            if (onlyWritableFields && fieldDefinition.isReadonly()) {
                continue;
            }
            String sourceFieldName = fieldDefinition.getName();
            String targetFieldName = toSalesforceFieldName(sourceFieldName, nameMappings);
            Object fieldValue = valueObject.getField(sourceFieldName);
            if (fieldValue != null) {
                if (fieldValue instanceof DynamicValueObject) {
                    fieldValue = toMap((DynamicValueObject) fieldValue, fieldDefinition.getNestedSchema(), nameMappings, onlyWritableFields);
                }
                if (fieldValue instanceof List && fieldDefinition.getType() == FieldType.String && fieldDefinition.isList()) {
                    fieldValue = String.join(";", (List) fieldValue);
                }
                map.put(targetFieldName, fieldValue);
            }
        }
        return map;
    }

    static public String toSalesforceFieldName(String entityFieldName, Map<String,String> nameMappings) {
        String salesforceFieldName = entityFieldName;
        if (nameMappings != null) {
            String mappedName = nameMappings.get(entityFieldName);
            if (mappedName != null) {
                salesforceFieldName = mappedName;
            }
        }
        return salesforceFieldName;
    }
}
