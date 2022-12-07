FROM node:19.2.0-slim AS install-ready

RUN npm install --global npm@9.1.3

# install dependencies first, in a different location for easier app bind mounting for local development
# due to default /opt permissions we have to create the dir with root and change perms
RUN mkdir /opt/node_app && chown node:node /opt/node_app
WORKDIR /opt/node_app

# the official node image provides an unprivileged user as a security best practice
# https://github.com/nodejs/docker-node/blob/master/docs/BestPractices.md#non-root-user
USER node
COPY package.json package-lock.json ./
ENV PATH /opt/node_app/node_modules/.bin:$PATH


FROM install-ready AS build

RUN npm ci

# copy and build source files
RUN mkdir /opt/node_app/app
WORKDIR /opt/node_app/app
COPY . .
RUN tsc --project . --outDir dist


FROM install-ready AS runtime

ARG NODE_ENV=production
ENV NODE_ENV $NODE_ENV

RUN npm ci

# set FORCE_COLOR for chalk colored logs
ENV FORCE_COLOR 1

COPY --from=build /opt/node_app/app/dist ./

# using npm start requires running with `docker run --init` in production
# and doesn't support graceful shutdown as easily
# https://github.com/nodejs/docker-node/blob/master/docs/BestPractices.md#cmd
ENTRYPOINT [ "node", "./taylorbot.js" ]
