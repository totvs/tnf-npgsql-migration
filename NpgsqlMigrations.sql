CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE "Authors" (
    "Id" bigserial NOT NULL,
    "Name" text NULL,
    "Birthdate" timestamp without time zone NOT NULL,
    "Ranking" smallint NOT NULL DEFAULT ('-1'::integer),
    CONSTRAINT "PK_Authors" PRIMARY KEY ("Id")
);

CREATE TABLE "Blogs" (
    "Id" serial NOT NULL,
    "Name" text NULL,
    "Description" text NULL,
    "Category" integer NOT NULL,
    CONSTRAINT "PK_Blogs" PRIMARY KEY ("Id")
);

CREATE TABLE "BlogAuthors" (
    "Id" serial NOT NULL,
    "AuthorId" bigint NOT NULL,
    "BlogId" integer NOT NULL,
    CONSTRAINT "PK_BlogAuthors" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BlogAuthors_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BlogAuthors_Blogs_BlogId" FOREIGN KEY ("BlogId") REFERENCES "Blogs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BlogPosts" (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Content" text NULL,
    "PublishDate" timestamp with time zone NOT NULL,
    "ReadTime" time without time zone NULL,
    "IsPublic" boolean NOT NULL DEFAULT (true),
    "BlogId" integer NOT NULL,
    "AuthorId" bigint NOT NULL,
    CONSTRAINT "PK_BlogPosts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BlogPosts_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BlogPosts_Blogs_BlogId" FOREIGN KEY ("BlogId") REFERENCES "Blogs" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BlogAuthors_AuthorId" ON "BlogAuthors" ("AuthorId");

CREATE INDEX "IX_BlogAuthors_BlogId" ON "BlogAuthors" ("BlogId");

CREATE INDEX "IX_BlogPosts_AuthorId" ON "BlogPosts" ("AuthorId");

CREATE INDEX "IX_BlogPosts_BlogId" ON "BlogPosts" ("BlogId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912225920_InitialModel', '3.1.28');

CREATE TABLE "AuthorMetrics" (
    "Id" serial NOT NULL,
    "AvgWordsPerPost" numeric NOT NULL,
    "AvgPostsPerMonth" numeric NOT NULL,
    "StarRating" real NOT NULL,
    "AuthorId" bigint NOT NULL,
    CONSTRAINT "PK_AuthorMetrics" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AuthorMetrics_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BlogPostMetrics" (
    "Id" serial NOT NULL,
    "ViewCount" bigint NOT NULL,
    "AvgViewCountPerDay" numeric NOT NULL,
    "PostId" uuid NOT NULL,
    CONSTRAINT "PK_BlogPostMetrics" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BlogPostMetrics_BlogPosts_PostId" FOREIGN KEY ("PostId") REFERENCES "BlogPosts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AuthorMetrics_AuthorId" ON "AuthorMetrics" ("AuthorId");

CREATE INDEX "IX_BlogPostMetrics_PostId" ON "BlogPostMetrics" ("PostId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912230114_AddingMetricTables', '3.1.28');

CREATE TABLE "BlogRatings" (
    "Id" serial NOT NULL,
    "StarRating" numeric NOT NULL,
    "BlogId" integer NOT NULL,
    CONSTRAINT "PK_BlogRatings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BlogRatings_Blogs_BlogId" FOREIGN KEY ("BlogId") REFERENCES "Blogs" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BlogRatings_BlogId" ON "BlogRatings" ("BlogId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232120_AddingBlogRatings', '3.1.28');

UPDATE "BlogRatings" SET "StarRating" = 0

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232132_ResetingBlogRatings', '3.1.28');

DROP TABLE "BlogRatings";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232153_DropBlogRatingsTable', '3.1.28');

