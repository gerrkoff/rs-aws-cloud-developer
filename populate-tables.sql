CREATE TABLE IF NOT EXISTS "users" (
                                       "id" varchar(100) PRIMARY KEY,
                                       "password" varchar(100) NOT NULL
);

INSERT INTO "users" ("id", "password") VALUES ('gerrkoff', 'TEST_PASSWORD') ON CONFLICT DO NOTHING;

CREATE TABLE IF NOT EXISTS "carts" (
                                       "id" uuid PRIMARY KEY,
                                       "user_id" varchar(100) NOT NULL,
                                       "created_at" timestamp NOT NULL,
                                       "updated_at" timestamp NOT NULL,
                                       "status" varchar(100) NOT NULL
);

ALTER TABLE "carts" DROP CONSTRAINT IF EXISTS "fk_user_id";
ALTER TABLE "carts" ADD CONSTRAINT "fk_user_id" FOREIGN KEY ("user_id") REFERENCES "users" ("id");

CREATE TABLE IF NOT EXISTS "cart_items" (
                                            "cart_id" uuid NOT NULL,
                                            "product_id" uuid NOT NULL,
                                            "count" int NOT NULL,
                                            "price" int NOT NULL
);

ALTER TABLE "cart_items" DROP CONSTRAINT IF EXISTS "fk_cart_id";
ALTER TABLE "cart_items" ADD CONSTRAINT "fk_cart_id" FOREIGN KEY ("cart_id") REFERENCES "carts" ("id");

CREATE TABLE IF NOT EXISTS "orders" (
                                        "id" uuid PRIMARY KEY,
                                        "user_id" varchar(100) NOT NULL,
                                        "payment" json NOT NULL,
                                        "delivery" json NOT NULL,
                                        "comments" varchar(100) NOT NULL,
                                        "status" varchar(100) NOT NULL,
                                        "total" int NOT NULL
);

ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "fk_cart_id";
ALTER TABLE "orders" ADD CONSTRAINT "fk_cart_id" FOREIGN KEY ("cart_id") REFERENCES "carts" ("id");
ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "fk_user_id";
ALTER TABLE "orders" ADD CONSTRAINT "fk_user_id" FOREIGN KEY ("user_id") REFERENCES "users" ("id");

INSERT INTO "carts" ("id", "user_id", "created_at", "updated_at", "status") VALUES ('f352749b-fd43-4883-9087-3b2ab76ee9eb', 'gerrkoff', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 'OPEN') ON CONFLICT DO NOTHING;
INSERT INTO "cart_items" ("cart_id", "product_id", "count", "price") VALUES ('f352749b-fd43-4883-9087-3b2ab76ee9eb', '60057269-4871-4375-8653-f076d1d2c9d0', 1, 100500) ON CONFLICT DO NOTHING;