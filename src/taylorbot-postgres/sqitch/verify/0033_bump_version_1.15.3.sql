-- Verify taylorbot-postgres:0032_bump_version_1.15.3 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.15.3';

    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.commands WHERE
        name = 'dailypayoutstreak'
    )));

    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.commands WHERE
        name = 'rankdailypayoutstreak'
    )));
END $$;

ROLLBACK;
