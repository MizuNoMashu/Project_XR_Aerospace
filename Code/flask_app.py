
from flask import Flask, send_file
from flask_restful import Resource, Api

app = Flask(__name__)
api = Api(app)

class myHUB(Resource):
    def get(self):
        return "hello"


    ##### Addressables ###############################################################################################
    @app.route("/WSAPlayer/<filename>", methods=['GET'])
    def get_addressable(filename):
        filepath = "/home/annacarini/mysite/WSAPlayer/" + filename
        return send_file(filepath)

    ############################################################################################################



api.add_resource(myHUB, '/')

if __name__ == '__main__':
    print('starting myHUB api... waiting')





