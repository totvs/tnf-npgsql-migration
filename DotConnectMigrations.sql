CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" ( 
  "MigrationId" varchar(150) NOT NULL,
  "ProductVersion" varchar(32) NOT NULL,
  PRIMARY KEY ("MigrationId")
);

CREATE TABLE "Authors" ( 
  "Id" bigserial NOT NULL,
  "Name" text NULL,
  "Birthdate" timestamp NOT NULL,
  "Ranking" smallint DEFAULT -1 NOT NULL,
  PRIMARY KEY ("Id")
);

CREATE TABLE "Blogs" ( 
  "Id" serial NOT NULL,
  "Name" text NULL,
  "Description" text NULL,
  "Category" int NOT NULL,
  PRIMARY KEY ("Id")
);

CREATE TABLE "BlogAuthors" ( 
  "Id" serial NOT NULL,
  "AuthorId" bigint NOT NULL,
  "BlogId" int NOT NULL,
  PRIMARY KEY ("Id"),
  CONSTRAINT "FK_BlogAuthors_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_BlogAuthors_Blogs_BlogId" FOREIGN KEY ("BlogId") REFERENCES "Blogs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BlogPosts" ( 
  "Id" uuid DEFAULT GEN_RANDOM_UUID() NOT NULL,
  "Content" text NULL,
  "PublishDate" timestamp with time zone NOT NULL,
  "ReadTime" time without time zone NULL,
  "IsPublic" boolean DEFAULT true NOT NULL,
  "BlogId" int NOT NULL,
  "AuthorId" bigint NOT NULL,
  PRIMARY KEY ("Id"),
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
  "AvgWordsPerPost" decimal NOT NULL,
  "AvgPostsPerMonth" decimal NOT NULL,
  "StarRating" real NOT NULL,
  "AuthorId" bigint NOT NULL,
  PRIMARY KEY ("Id"),
  CONSTRAINT "FK_AuthorMetrics_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BlogPostMetrics" ( 
  "Id" serial NOT NULL,
  "ViewCount" bigint NOT NULL,
  "AvgViewCountPerDay" decimal NOT NULL,
  "PostId" uuid NOT NULL,
  PRIMARY KEY ("Id"),
  CONSTRAINT "FK_BlogPostMetrics_BlogPosts_PostId" FOREIGN KEY ("PostId") REFERENCES "BlogPosts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AuthorMetrics_AuthorId" ON "AuthorMetrics" ("AuthorId");

CREATE INDEX "IX_BlogPostMetrics_PostId" ON "BlogPostMetrics" ("PostId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912230114_AddingMetricTables', '3.1.28');

CREATE TABLE "BlogRatings" ( 
  "Id" serial NOT NULL,
  "StarRating" decimal NOT NULL,
  "BlogId" int NOT NULL,
  PRIMARY KEY ("Id"),
  CONSTRAINT "FK_BlogRatings_Blogs_BlogId" FOREIGN KEY ("BlogId") REFERENCES "Blogs" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BlogRatings_BlogId" ON "BlogRatings" ("BlogId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232120_AddingBlogRatings', '3.1.28');

UPDATE "BlogRatings" SET "StarRating" = 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232132_ResetingBlogRatings', '3.1.28');

DROP TABLE "BlogRatings" CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220912232153_DropBlogRatingsTable', '3.1.28');

