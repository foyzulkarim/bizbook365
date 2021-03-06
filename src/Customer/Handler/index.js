const express = require('express')
var amqp = require('amqplib/callback_api');

const app = express()
const port = 51001;
let rabbitMQChannel = null;
const exchange = 'events';

app.get('/api/rabbitmessage', (req, res) => {
    let msg = 'awesome message from node rabbitmq';
    //var queue = 'hello';
    //rabbitMQChannel.sendToQueue(queue, Buffer.from(msg));
    rabbitMQChannel.publish(exchange, '', Buffer.from(msg));
    console.log(" [x] Sent %s", msg);
    res.send('Thank you!');
});

app.get('/', (req, res) => {
    res.send('Hello World!');
});

app.listen(port, () => {
    console.log(`Example app listening at http://localhost:${port}`);

    amqp.connect('amqp://localhost', function (error0, connection) {
        if (error0) {
            throw error0;
        }

        connection.createChannel(function (error1, channel) {
            if (error1) {
                throw error1;
            }

            channel.assertExchange(exchange, 'fanout', {
                durable: false
            });

            var queue = 'hello-x';

            channel.assertQueue(queue, {
                durable: false
            }, function (error2, q) {
                if (error2) {
                    throw error2;
                }
                console.log(" [*] Waiting for messages in %s. To exit press CTRL+C", q.queue);
                channel.bindQueue(q.queue, exchange, '');

                channel.consume(q.queue, function (msg) {
                    if (msg.content) {
                        console.log(" [x] %s", msg.content.toString());
                    }
                }, {
                    noAck: true
                });
            });

            rabbitMQChannel = channel;
        });
    });


})