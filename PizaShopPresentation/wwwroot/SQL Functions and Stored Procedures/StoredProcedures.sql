-- /////////////////////////////////////////////////////////////////////////KOT////////////////////////////////////////////////////////////////////////////////////////////////

-- PROCEDURE: public.update_order_item_statuses(integer, jsonb, character varying)

-- DROP PROCEDURE IF EXISTS public.update_order_item_statuses(integer, jsonb, character varying);

CREATE OR REPLACE PROCEDURE public.update_order_item_statuses(
	IN p_order_id integer,
	IN p_items jsonb,
	IN p_new_status character varying,
	OUT p_success boolean,
	OUT p_rows_affected integer)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_item RECORD;
    v_original_ready_quantity INTEGER;
    v_original_item_status VARCHAR;
    v_new_ready_quantity INTEGER;
    v_reduction INTEGER;
BEGIN
    p_success := FALSE;
    p_rows_affected := 0;

    -- Validate inputs
    IF p_items IS NULL OR jsonb_array_length(p_items) = 0 THEN
        RAISE EXCEPTION 'No items provided for update.';
    END IF;

    -- Loop through each item in the JSON array
    FOR v_item IN (
        SELECT (elem->>'OrderItemId')::INTEGER AS order_item_id,
               (elem->>'AdjustedQuantity')::INTEGER AS adjusted_quantity
        FROM jsonb_array_elements(p_items) AS elem
    )
    LOOP
        -- Fetch the existing order item
        SELECT ready_quantity, item_status
        INTO v_original_ready_quantity, v_original_item_status
        FROM order_items
        WHERE id = v_item.order_item_id AND order_id = p_order_id
        FOR UPDATE;

        IF NOT FOUND THEN
            RAISE EXCEPTION 'No matching order item found for OrderItemId: %', v_item.order_item_id;
        END IF;

        -- Update logic based on new_status
        IF p_new_status = 'ready' THEN
            -- Calculate new ready quantity (cannot exceed total quantity)
            v_new_ready_quantity := LEAST(v_original_ready_quantity + v_item.adjusted_quantity, (SELECT quantity FROM order_items WHERE id = v_item.order_item_id));
            
            UPDATE order_items
            SET ready_quantity = v_new_ready_quantity,
                item_status = CASE WHEN v_new_ready_quantity >= quantity THEN 'ready' ELSE 'in_progress' END,
                ready_at = CASE WHEN v_new_ready_quantity > v_original_ready_quantity AND ready_at IS NULL THEN NOW() AT TIME ZONE 'Asia/Kolkata' END
            WHERE id = v_item.order_item_id AND order_id = p_order_id;

        ELSIF p_new_status = 'in_progress' THEN
            -- Calculate reduction (cannot reduce below 0)
            v_reduction := LEAST(v_item.adjusted_quantity, v_original_ready_quantity);
            v_new_ready_quantity := v_original_ready_quantity - v_reduction;

            UPDATE order_items
            SET ready_quantity = v_new_ready_quantity,
                item_status = CASE WHEN v_new_ready_quantity < quantity THEN 'in_progress' ELSE 'ready' END,
                ready_at = CASE WHEN v_new_ready_quantity = 0 THEN NULL ELSE ready_at END
            WHERE id = v_item.order_item_id AND order_id = p_order_id;

        END IF;

        p_rows_affected := p_rows_affected + 1;
    END LOOP;

    -- If updates were made, set success to true
    IF p_rows_affected > 0 THEN
        p_success := TRUE;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        p_success := FALSE;
        p_rows_affected := 0;
        RAISE NOTICE 'Error: %', SQLERRM;
END;
$BODY$;
ALTER PROCEDURE public.update_order_item_statuses(integer, jsonb, character varying)
    OWNER TO postgres;

-- /////////////////////////////////////////////////////////////////////////WaitingList///////////////////////////////////////////////////////////////////////////////////////////////////

-- PROCEDURE: public.add_waiting_token(character varying, character varying, character varying, integer, integer, character varying)

-- DROP PROCEDURE IF EXISTS public.add_waiting_token(character varying, character varying, character varying, integer, integer, character varying);

CREATE OR REPLACE PROCEDURE public.add_waiting_token(
	IN p_customer_name character varying,
	IN p_email character varying,
	IN p_phone_number character varying,
	IN p_num_of_persons integer,
	IN p_section_id integer,
	IN p_status character varying)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_customer_exists INTEGER;
BEGIN
    -- Check if customer exists by email
    SELECT COUNT(*) INTO v_customer_exists
    FROM customers
    WHERE email = p_email;

    -- If customer does not exist, create a new customer
    IF v_customer_exists = 0 THEN
        INSERT INTO customers (name, email, phone_no, no_of_persons, date)
        VALUES (
            p_customer_name,
            p_email,
            p_phone_number,
            p_num_of_persons,
            CURRENT_DATE
        );
    END IF;

    -- Insert the waiting token
    INSERT INTO waiting_tokens (
        customer_name,
        email,
        phone_number,
        num_of_persons,
        section_id,
        status,
        created_at,
        is_deleted,
        is_assigned
    )
    VALUES (
        p_customer_name,
        p_email,
        p_phone_number,
        p_num_of_persons,
        p_section_id,
        p_status,
        NOW() AT TIME ZONE 'Asia/Kolkata',
        FALSE,
        FALSE
    );
END;
$BODY$;
ALTER PROCEDURE public.add_waiting_token(character varying, character varying, character varying, integer, integer, character varying)
    OWNER TO postgres;


-- PROCEDURE: public.update_waiting_token(integer, character varying, character varying, character varying, integer, integer)

-- DROP PROCEDURE IF EXISTS public.update_waiting_token(integer, character varying, character varying, character varying, integer, integer);

CREATE OR REPLACE PROCEDURE public.update_waiting_token(
	IN p_id integer,
	IN p_customer_name character varying,
	IN p_email character varying,
	IN p_phone_number character varying,
	IN p_num_of_persons integer,
	IN p_section_id integer)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    -- Update the waiting token
    UPDATE waiting_tokens
    SET
        customer_name = p_customer_name,
        email = p_email,
        phone_number = p_phone_number,
        num_of_persons = p_num_of_persons,
        section_id = p_section_id
    WHERE id = p_id;
END;
$BODY$;
ALTER PROCEDURE public.update_waiting_token(integer, character varying, character varying, character varying, integer, integer)
    OWNER TO postgres;


-- PROCEDURE: public.delete_waiting_token(integer)

-- DROP PROCEDURE IF EXISTS public.delete_waiting_token(integer);

CREATE OR REPLACE PROCEDURE public.delete_waiting_token(
	IN p_id integer)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    -- Mark the waiting token as deleted
    UPDATE waiting_tokens
    SET is_deleted = TRUE
    WHERE id = p_id;
END;
$BODY$;
ALTER PROCEDURE public.delete_waiting_token(integer)
    OWNER TO postgres;



-- PROCEDURE: public.assign_table(integer[], integer, integer, character varying, character varying, character varying, integer)

-- DROP PROCEDURE IF EXISTS public.assign_table(integer[], integer, integer, character varying, character varying, character varying, integer);

CREATE OR REPLACE PROCEDURE public.assign_table(
	IN p_selected_table_ids integer[],
	IN p_section_id integer,
	IN p_waiting_token_id integer,
	IN p_email character varying,
	IN p_name character varying,
	IN p_phone_number character varying,
	IN p_num_of_persons integer,
	OUT p_order_id integer)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_customer_id INTEGER;
    v_table_id INTEGER;
    v_tax RECORD;
BEGIN
    -- Check if any of the selected tables are unavailable
    IF EXISTS (
        SELECT 1
        FROM tables t
        WHERE t.id = ANY(p_selected_table_ids)
        AND t.status != 'available'
    ) THEN
        RAISE EXCEPTION 'One or more selected tables are no longer available.';
    END IF;

    -- Check if customer exists, if not create a new one
    SELECT id INTO v_customer_id
    FROM customers
    WHERE email = p_email;

    IF v_customer_id IS NULL THEN
        INSERT INTO customers (name, email, phone_no, no_of_persons, date)
        VALUES (p_name, p_email, p_phone_number, p_num_of_persons, CURRENT_DATE)
        RETURNING id INTO v_customer_id;
    END IF;

    -- Create a new order
    INSERT INTO orders (
        customer_id, total_amount, invoice_no, order_type, order_status,
        created_at, updated_at
    )
    VALUES (
        v_customer_id, 0, CONCAT('DOM0', v_customer_id), 'DineIn', 'pending',
        NOW() AT TIME ZONE 'Asia/Kolkata', NOW() AT TIME ZONE 'Asia/Kolkata'
    )
    RETURNING id INTO p_order_id;

    -- Add applicable taxes to order_tax
    FOR v_tax IN (
        SELECT id, type, value
        FROM taxes_fees
        WHERE is_enabled = true AND is_deleted = false
    ) LOOP
        INSERT INTO order_tax (order_id, tax_id, tax_percentage, tax_flat, is_applied)
        VALUES (
            p_order_id,
            v_tax.id,
            CASE WHEN v_tax.type = 'percentage' THEN v_tax.value ELSE NULL END,
            CASE WHEN v_tax.type = 'fixed' THEN v_tax.value ELSE NULL END,
            true
        );
    END LOOP;

    -- Update waiting token if provided
    IF p_waiting_token_id IS NOT NULL THEN
        UPDATE waiting_tokens
        SET is_assigned = true
        WHERE id = p_waiting_token_id;
    END IF;

    -- Update table statuses and create order_table entries
    FOREACH v_table_id IN ARRAY p_selected_table_ids LOOP
        -- Update table status to 'reserved'
        UPDATE tables
        SET status = 'reserved'
        WHERE id = v_table_id;

        -- Create order_table entry
        INSERT INTO order_tables (order_id, table_id)
        VALUES (p_order_id, v_table_id);
    END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '%', SQLERRM;
END;
$BODY$;
ALTER PROCEDURE public.assign_table(integer[], integer, integer, character varying, character varying, character varying, integer)
    OWNER TO postgres;
