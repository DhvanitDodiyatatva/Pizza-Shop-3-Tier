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

-- /////////////////////////////////////////////////////////////////////////Menu///////////////////////////////////////////////////////////////////////////////////////////////////

-- PROCEDURE: public.toggle_favorite_item(integer)

-- DROP PROCEDURE IF EXISTS public.toggle_favorite_item(integer);

CREATE OR REPLACE PROCEDURE public.toggle_favorite_item(
	IN p_item_id integer,
	OUT p_new_favorite_status boolean)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_item_exists BOOLEAN;
BEGIN
    -- Check if the item exists and is not deleted
    SELECT EXISTS (
        SELECT 1
        FROM items
        WHERE id = p_item_id AND is_deleted = FALSE
    ) INTO v_item_exists;

    IF NOT v_item_exists THEN
        RAISE EXCEPTION 'Item not found or has been deleted.';
    END IF;

    -- Toggle the favorite status and update the timestamp
    UPDATE items
    SET 
        is_favourite = NOT is_favourite,
        updated_at = NOW() AT TIME ZONE 'Asia/Kolkata'
    WHERE id = p_item_id
    RETURNING is_favourite INTO p_new_favorite_status;

EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '%', SQLERRM;
END;
$BODY$;
ALTER PROCEDURE public.toggle_favorite_item(integer)
    OWNER TO postgres;


-- PROCEDURE: public.save_order(integer, numeric, character varying, jsonb, jsonb)

-- DROP PROCEDURE IF EXISTS public.save_order(integer, numeric, character varying, jsonb, jsonb);

CREATE OR REPLACE PROCEDURE public.save_order(
	IN p_order_id integer,
	IN p_total_amount numeric,
	IN p_payment_method character varying,
	IN p_cart_items jsonb,
	IN p_tax_settings jsonb)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_cart_item JSONB;
    v_modifier JSONB;
    v_tax_name TEXT;
    v_is_applied BOOLEAN;
    v_existing_item RECORD;
    v_key TEXT;
    v_order_item_id INTEGER;
    v_quantity_increased BOOLEAN;
    v_item_exists BOOLEAN;
BEGIN
    -- 1. Update Order table
    UPDATE orders
    SET order_status = 'in_progress',
        total_amount = p_total_amount,
        payment_method = p_payment_method,
        updated_at = NOW() AT TIME ZONE 'Asia/Kolkata'
    WHERE id = p_order_id;

    -- 2. Update OrderTax table
    FOR v_tax_name, v_is_applied IN
        SELECT key, value::BOOLEAN
        FROM jsonb_each(p_tax_settings)
    LOOP
        UPDATE order_tax ot
        SET is_applied = CASE
            WHEN ot.tax_percentage IS NOT NULL THEN TRUE
            ELSE v_is_applied
        END
        FROM taxes_fees tf
        WHERE ot.order_id = p_order_id
        AND ot.tax_id = tf.id
        AND tf.name = v_tax_name;
    END LOOP;

    -- 3. Update Table table
    UPDATE tables t
    SET status = 'occupied'
    FROM order_tables ot
    WHERE ot.order_id = p_order_id
    AND ot.table_id = t.id;

    -- 4 & 5. Update OrderItem and OrderItemModifier tables
    -- First, identify items to remove
    FOR v_existing_item IN
        SELECT oi.id, oi.item_id, 
               STRING_AGG(oim.modifier_id::TEXT, ',' ORDER BY oim.modifier_id) AS modifier_key
        FROM order_items oi
        LEFT JOIN order_item_modifiers oim ON oi.id = oim.order_item_id
        WHERE oi.order_id = p_order_id
        GROUP BY oi.id, oi.item_id
    LOOP
        v_key := v_existing_item.item_id || '-' || COALESCE(v_existing_item.modifier_key, '');

        -- Compute the modifier keys for cart items and check existence
        WITH cart_item_modifiers AS (
            SELECT (ci->>'ItemId')::INTEGER AS item_id,
                   (ci->>'ItemId')::TEXT || '-' || COALESCE((
                       SELECT STRING_AGG((m->>'ModifierId')::TEXT, ',' ORDER BY (m->>'ModifierId')::INTEGER)
                       FROM jsonb_array_elements(ci->'Modifiers') m
                   ), '') AS cart_key
            FROM jsonb_array_elements(p_cart_items) ci
        )
        SELECT EXISTS (
            SELECT 1
            FROM cart_item_modifiers cim
            WHERE cim.item_id = v_existing_item.item_id
            AND cim.cart_key = v_key
        ) INTO v_item_exists;

        IF NOT v_item_exists THEN
            -- Delete associated modifiers
            DELETE FROM order_item_modifiers
            WHERE order_item_id = v_existing_item.id;
            
            -- Delete the order item
            DELETE FROM order_items
            WHERE id = v_existing_item.id;
        END IF;
    END LOOP;

    -- Add or update items
    FOR v_cart_item IN SELECT * FROM jsonb_array_elements(p_cart_items)
    LOOP
        v_key := (v_cart_item->>'ItemId')::TEXT || '-' || COALESCE((
            SELECT STRING_AGG((m->>'ModifierId')::TEXT, ',' ORDER BY (m->>'ModifierId')::INTEGER)
            FROM jsonb_array_elements(v_cart_item->'Modifiers') m
        ), '');

        -- Check if item exists by computing the modifier key in a subquery
        SELECT oi.id, oi.quantity
        INTO v_existing_item
        FROM order_items oi
        LEFT JOIN (
            SELECT oim2.order_item_id,
                   STRING_AGG(oim2.modifier_id::TEXT, ',' ORDER BY oim2.modifier_id) AS modifier_key
            FROM order_item_modifiers oim2
            GROUP BY oim2.order_item_id
        ) oim ON oi.id = oim.order_item_id
        WHERE oi.order_id = p_order_id
        AND oi.item_id = (v_cart_item->>'ItemId')::INTEGER
        AND v_key = ((v_cart_item->>'ItemId')::TEXT || '-' || COALESCE(oim.modifier_key, ''));

        IF v_existing_item IS NULL THEN
            -- Add new OrderItem
            INSERT INTO order_items (
                order_id, item_id, quantity, unit_price, total_price,
                item_status, ready_quantity, created_at
            )
            VALUES (
                p_order_id,
                (v_cart_item->>'ItemId')::INTEGER,
                (v_cart_item->>'Quantity')::INTEGER,
                (v_cart_item->>'UnitPrice')::DECIMAL,
                (v_cart_item->>'TotalPrice')::DECIMAL,
                'in_progress',
                0,
                NOW() AT TIME ZONE 'Asia/Kolkata'
            );

            -- Add OrderItemModifiers
            INSERT INTO order_item_modifiers (
                order_item_id, modifier_id, quantity, price
            )
            SELECT 
                currval('order_items_id_seq'),
                (modifier->>'ModifierId')::INTEGER,
                (modifier->>'Quantity')::INTEGER,
                (modifier->>'Price')::DECIMAL
            FROM jsonb_array_elements(v_cart_item->'Modifiers') modifier;

        ELSE
            -- Update existing OrderItem
            v_quantity_increased := (v_cart_item->>'Quantity')::INTEGER > v_existing_item.quantity;
            
            UPDATE order_items
            SET quantity = (v_cart_item->>'Quantity')::INTEGER,
                unit_price = (v_cart_item->>'UnitPrice')::DECIMAL,
                total_price = (v_cart_item->>'TotalPrice')::DECIMAL,
                item_status = CASE 
                    WHEN v_quantity_increased THEN 'in_progress'
                    ELSE item_status
                END
            WHERE id = v_existing_item.id;

            -- Update OrderItemModifiers
            FOR v_modifier IN
                SELECT * FROM jsonb_array_elements(v_cart_item->'Modifiers')
            LOOP
                -- Check if modifier exists
                IF NOT EXISTS (
                    SELECT 1
                    FROM order_item_modifiers oim
                    WHERE oim.order_item_id = v_existing_item.id
                    AND oim.modifier_id = (v_modifier->>'ModifierId')::INTEGER
                ) THEN
                    -- Add new modifier
                    INSERT INTO order_item_modifiers (
                        order_item_id, modifier_id, quantity, price
                    )
                    VALUES (
                        v_existing_item.id,
                        (v_modifier->>'ModifierId')::INTEGER,
                        (v_modifier->>'Quantity')::INTEGER,
                        (v_modifier->>'Price')::DECIMAL
                    );
                ELSE
                    -- Update existing modifier
                    UPDATE order_item_modifiers
                    SET quantity = (v_modifier->>'Quantity')::INTEGER,
                        price = (v_modifier->>'Price')::DECIMAL
                    WHERE order_item_id = v_existing_item.id
                    AND modifier_id = (v_modifier->>'ModifierId')::INTEGER;
                END IF;
            END LOOP;

            -- Remove modifiers that are no longer present
            DELETE FROM order_item_modifiers oim
            WHERE oim.order_item_id = v_existing_item.id
            AND oim.modifier_id NOT IN (
                SELECT (modifier->>'ModifierId')::INTEGER
                FROM jsonb_array_elements(v_cart_item->'Modifiers') modifier
            );
        END IF;
    END LOOP;
END;
$BODY$;
ALTER PROCEDURE public.save_order(integer, numeric, character varying, jsonb, jsonb)
    OWNER TO postgres;


-- PROCEDURE: public.complete_order(integer)

-- DROP PROCEDURE IF EXISTS public.complete_order(integer);

CREATE OR REPLACE PROCEDURE public.complete_order(
	IN p_order_id integer)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_customer_id INTEGER;
    v_total_orders INTEGER;
    v_all_items_ready BOOLEAN;
BEGIN
    -- 1. Validate the order exists and fetch customer_id
    SELECT customer_id INTO v_customer_id
    FROM orders
    WHERE id = p_order_id;

    IF v_customer_id IS NULL THEN
        RAISE EXCEPTION 'Order not found.';
    END IF;

    -- 2. Check if all items are ready
    SELECT BOOL_AND(item_status = 'ready') INTO v_all_items_ready
    FROM order_items
    WHERE order_id = p_order_id;

    IF NOT v_all_items_ready THEN
        RAISE EXCEPTION 'Cannot complete order: Some items are not ready.';
    END IF;

    -- 3. Fetch and update customer's TotalOrders
    SELECT total_orders INTO v_total_orders
    FROM customers
    WHERE id = v_customer_id;

    IF v_total_orders IS NULL THEN
        RAISE EXCEPTION 'Customer not found.';
    END IF;

    UPDATE customers
    SET total_orders = COALESCE(v_total_orders, 0) + 1
    WHERE id = v_customer_id;

    -- 4. Update Order table
    UPDATE orders
    SET order_status = 'completed',
        payment_status = 'paid',
        updated_at = NOW() AT TIME ZONE 'Asia/Kolkata'
    WHERE id = p_order_id;

    -- 5. Update Table table
    UPDATE tables t
    SET status = 'available'
    FROM order_tables ot
    WHERE ot.order_id = p_order_id
    AND ot.table_id = t.id;

    -- 6. Update OrderItem table
    UPDATE order_items
    SET item_status = 'served'
    WHERE order_id = p_order_id;
END;
$BODY$;
ALTER PROCEDURE public.complete_order(integer)
    OWNER TO postgres;


-- PROCEDURE: public.cancel_order(integer)

-- DROP PROCEDURE IF EXISTS public.cancel_order(integer);

CREATE OR REPLACE PROCEDURE public.cancel_order(
	IN p_order_id integer)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_all_items_in_progress BOOLEAN;
BEGIN
    -- 1. Validate the order exists
    IF NOT EXISTS (
        SELECT 1
        FROM orders
        WHERE id = p_order_id
    ) THEN
        RAISE EXCEPTION 'Order not found.';
    END IF;

    -- 2. Check if all items are in_progress
    SELECT BOOL_AND(item_status = 'in_progress') INTO v_all_items_in_progress
    FROM order_items
    WHERE order_id = p_order_id;

    IF NOT v_all_items_in_progress THEN
        RAISE EXCEPTION 'Cannot cancel order: Some items are not in progress.';
    END IF;

    -- 3. Update Order table
    UPDATE orders
    SET order_status = 'cancelled',
        payment_status = 'failed',
        updated_at = NOW() AT TIME ZONE 'Asia/Kolkata'
    WHERE id = p_order_id;

    -- 4. Update Table table
    UPDATE tables t
    SET status = 'available'
    FROM order_tables ot
    WHERE ot.order_id = p_order_id
    AND ot.table_id = t.id;

    -- 5. Update OrderItem table
    UPDATE order_items
    SET item_status = 'served'
    WHERE order_id = p_order_id;
END;
$BODY$;
ALTER PROCEDURE public.cancel_order(integer)
    OWNER TO postgres;


-- PROCEDURE: public.update_customer_details(integer, character varying, character varying, character varying, integer)

-- DROP PROCEDURE IF EXISTS public.update_customer_details(integer, character varying, character varying, character varying, integer);

CREATE OR REPLACE PROCEDURE public.update_customer_details(
	IN p_customer_id integer,
	IN p_name character varying,
	IN p_email character varying,
	IN p_phone_no character varying,
	IN p_no_of_persons integer)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    -- 1. Check if email already exists for a different customer
    IF EXISTS (
        SELECT 1
        FROM customers
        WHERE email = p_email
        AND id != p_customer_id
    ) THEN
        RAISE EXCEPTION 'This email already exists.';
    END IF;

    -- 2. Check if the customer exists
    IF NOT EXISTS (
        SELECT 1
        FROM customers
        WHERE id = p_customer_id
    ) THEN
        RAISE EXCEPTION 'Customer not found.';
    END IF;

    -- 3. Update the customer details
    UPDATE customers
    SET name = p_name,
        email = p_email,
        phone_no = p_phone_no,
        no_of_persons = p_no_of_persons
    WHERE id = p_customer_id;
END;
$BODY$;
ALTER PROCEDURE public.update_customer_details(integer, character varying, character varying, character varying, integer)
    OWNER TO postgres;


-- PROCEDURE: public.save_order_instructions(integer, character varying)

-- DROP PROCEDURE IF EXISTS public.save_order_instructions(integer, character varying);

CREATE OR REPLACE PROCEDURE public.save_order_instructions(
	IN p_order_id integer,
	IN p_order_instructions character varying)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    -- Check if the order exists
    IF NOT EXISTS (
        SELECT 1
        FROM orders
        WHERE id = p_order_id
    ) THEN
        RAISE EXCEPTION 'Order not found.';
    END IF;

    -- Update the order instructions and timestamp
    UPDATE orders
    SET order_instructions = NULLIF(p_order_instructions, ''),
        updated_at = NOW() AT TIME ZONE 'Asia/Kolkata'
    WHERE id = p_order_id;
END;
$BODY$;
ALTER PROCEDURE public.save_order_instructions(integer, character varying)
    OWNER TO postgres;



-- PROCEDURE: public.save_special_instructions(integer, character varying)

-- DROP PROCEDURE IF EXISTS public.save_special_instructions(integer, character varying);

CREATE OR REPLACE PROCEDURE public.save_special_instructions(
	IN p_order_item_id integer,
	IN p_special_instructions character varying)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    -- Check if the order item exists
    IF NOT EXISTS (
        SELECT 1
        FROM order_items
        WHERE id = p_order_item_id
    ) THEN
        RAISE EXCEPTION 'Order item not found.';
    END IF;

    -- Update the special instructions
    UPDATE order_items
    SET special_instructions = NULLIF(p_special_instructions, '')
    WHERE id = p_order_item_id;
END;
$BODY$;
ALTER PROCEDURE public.save_special_instructions(integer, character varying)
    OWNER TO postgres;



-- PROCEDURE: public.save_customer_review(integer, integer, integer, integer, character varying)

-- DROP PROCEDURE IF EXISTS public.save_customer_review(integer, integer, integer, integer, character varying);

CREATE OR REPLACE PROCEDURE public.save_customer_review(
	IN p_order_id integer,
	IN p_food_rating integer,
	IN p_service_rating integer,
	IN p_ambience_rating integer,
	IN p_comment character varying)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_average_rating DECIMAL;
BEGIN
    -- Check if the order exists
    IF NOT EXISTS (
        SELECT 1
        FROM orders
        WHERE id = p_order_id
    ) THEN
        RAISE EXCEPTION 'Order not found.';
    END IF;

    -- Calculate the average rating
    v_average_rating := (p_food_rating + p_service_rating + p_ambience_rating) / 3.0;

    -- Insert the customer review
    INSERT INTO customer_reviews (
        order_id,
        food_rating,
        service_rating,
        ambience_rating,
        comment,
        created_at
    )
    VALUES (
        p_order_id,
        p_food_rating,
        p_service_rating,
        p_ambience_rating,
        NULLIF(p_comment, ''),
        NOW() AT TIME ZONE 'Asia/Kolkata'
    );

    -- Update the order with the average rating
    UPDATE orders
    SET rating = v_average_rating
    WHERE id = p_order_id;
END;
$BODY$;
ALTER PROCEDURE public.save_customer_review(integer, integer, integer, integer, character varying)
    OWNER TO postgres;


