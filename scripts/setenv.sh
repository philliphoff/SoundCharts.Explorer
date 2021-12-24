#!/bin/bash

THISDIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

export $(cat "$THISDIR/../secrets.json" | jq -r "to_entries|map(\"\(.key)=\(.value|tostring)\")|.[]")
