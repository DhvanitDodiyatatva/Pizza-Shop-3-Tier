-- /////////////////////////////////////////////////////////////////////////KOT///////////////////////////////////////////////////////////////////////////////////////////////////

-- FUNCTION: public.get_orders_by_category_and_status(integer, character varying)

-- DROP FUNCTION IF EXISTS public.get_orders_by_category_and_status(integer, character varying);

CREATE OR REPLACE FUNCTION public.get_orders_by_category_and_status(
	p_category_id integer,
	p_status character varying)
    RETURNS TABLE(order_id integer, order_created_at timestamp without time zone, order_instructions text, order_item_id integer, order_item_quantity integer, order_item_ready_quantity integer, order_item_special_instructions text, item_name character varying, category_name character varying, modifier_names character varying[], table_name character varying, section_name character varying) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
BEGIN
    RETURN QUERY
    SELECT 
        o.id AS order_id,
        o.created_at AS order_created_at,
        o.order_instructions AS order_instructions,
        oi.id AS order_item_id,
        oi.quantity AS order_item_quantity,
        oi.ready_quantity AS order_item_ready_quantity,
        oi.special_instructions AS order_item_special_instructions,
        i.name AS item_name,
        c.name AS category_name,
        ARRAY(
            SELECT m.name
            FROM order_item_modifiers oim
            JOIN modifiers m ON oim.modifier_id = m.id
            WHERE oim.order_item_id = oi.id AND m.name IS NOT NULL
        ) AS modifier_names,
        t.name AS table_name,
        s.name AS section_name
    FROM orders o
    JOIN order_items oi ON o.id = oi.order_id
    JOIN items i ON oi.item_id = i.id
    LEFT JOIN categories c ON i.category_id = c.id
    LEFT JOIN order_tables ot ON o.id = ot.order_id
    LEFT JOIN tables t ON ot.table_id = t.id
    LEFT JOIN sections s ON t.section_id = s.id
    WHERE 
        (p_category_id IS NULL OR i.category_id = p_category_id)
        AND (
            p_status IS NULL 
            OR oi.item_status = p_status 
            OR (oi.item_status = 'in_progress')
        )
        AND oi.id IS NOT NULL; -- Ensure there are order items
END;
$BODY$;

ALTER FUNCTION public.get_orders_by_category_and_status(integer, character varying)
    OWNER TO postgres;



-- FUNCTION: public.get_order_for_kot_details(integer)

-- DROP FUNCTION IF EXISTS public.get_order_for_kot_details(integer);

CREATE OR REPLACE FUNCTION public.get_order_for_kot_details(
	p_order_id integer)
    RETURNS TABLE(order_id integer, order_item_id integer, item_name character varying, quantity integer, ready_quantity integer, item_status character varying, category_name character varying, modifier_names character varying[]) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
BEGIN
    RETURN QUERY
    SELECT 
        o.id AS order_id,
        oi.id AS order_item_id,
        i.name AS item_name,
        oi.quantity AS quantity,
        oi.ready_quantity AS ready_quantity,
        oi.item_status AS item_status,
        c.name AS category_name,
        ARRAY(
            SELECT m.name
            FROM order_item_modifiers oim
            JOIN modifiers m ON oim.modifier_id = m.id
            WHERE oim.order_item_id = oi.id AND m.name IS NOT NULL
        ) AS modifier_names
    FROM orders o
    JOIN order_items oi ON o.id = oi.order_id
    JOIN items i ON oi.item_id = i.id
    LEFT JOIN categories c ON i.category_id = c.id
    WHERE o.id = p_order_id;
END;
$BODY$;

ALTER FUNCTION public.get_order_for_kot_details(integer)
    OWNER TO postgres;


-- /////////////////////////////////////////////////////////////////////////WaitingList/////////////////////////////////////////////////////////////////////////////////////////////////


-- FUNCTION: public.get_waiting_tokens_with_sections()

-- DROP FUNCTION IF EXISTS public.get_waiting_tokens_with_sections();

CREATE OR REPLACE FUNCTION public.get_waiting_tokens_with_sections(
	)
    RETURNS TABLE(id integer, customername character varying, phonenumber character varying, email character varying, numofpersons integer, sectionid integer, status character varying, createdat timestamp without time zone, isdeleted boolean, isassigned boolean, sectionname character varying) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
BEGIN
    RETURN QUERY
    SELECT 
        wt.id AS "Id",
        wt.customer_name AS "CustomerName",
        wt.phone_number AS "PhoneNumber",
        wt.email AS "Email",
        wt.num_of_persons AS "NumOfPersons",
        wt.section_id AS "SectionId",
        wt.status AS "Status",
        wt.created_at AS "CreatedAt",
        wt.is_deleted AS "IsDeleted",
        wt.is_assigned AS "IsAssigned",
        s.name AS "SectionName"
    FROM waiting_tokens wt
    LEFT JOIN sections s ON wt.section_id = s.id
    WHERE wt.is_deleted = FALSE AND wt.is_assigned = FALSE;
END;
$BODY$;

ALTER FUNCTION public.get_waiting_tokens_with_sections()
    OWNER TO postgres;
