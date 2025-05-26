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

