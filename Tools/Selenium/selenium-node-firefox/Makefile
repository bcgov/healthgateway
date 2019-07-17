build:
	docker build -t "jr00n/node-firefox-os" -f Dockerfile .

.PHONY:test
test:
	docker build -t "jr00n/node-firefox-os-candidate" -f Dockerfile .
	sh test/run
