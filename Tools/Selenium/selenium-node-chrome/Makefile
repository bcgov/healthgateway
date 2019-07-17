build:
	docker build -t "jr00n/node-chrome-os" -f Dockerfile .

.PHONY:test
test:
	docker build -t "jr00n/node-chrome-os-candidate" -f Dockerfile .
	sh test/run
