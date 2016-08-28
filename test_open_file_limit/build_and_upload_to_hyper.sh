#!/bin/bash

docker build --tag test-file-limit --tag kjksf/test-file-limit .
docker push kjksf/test-file-limit
hyper pull kjksf/test-file-limit
