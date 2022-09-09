#
provide environment variables as secret files for docker:

azure appconfig
postgres username
postgres password

	in the appconfig these will be required:

	```
		chmod +x scripts/define-secrets.sh
		scripts/define-secrets.sh
	```

configure azure appconfig with additional environment variables:

currently available:

APPLICATION:POSTGRESOPTIONS:CONNECTIONSTRING
APPLICATION:COSMOSOPTIONS:ENDPOINT
APPLICATION:COSMOSOPTIONS:KEY


some env vars may be required for appsettings.json while generating efcore migrations