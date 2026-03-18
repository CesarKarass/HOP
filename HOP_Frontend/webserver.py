from flask import Flask, render_template, request, redirect, url_for

app = Flask(__name__)

datos_empresa_global = None

@app.route("/")
def Pagina_principal():
    return render_template("index.html")

@app.route("/inicio")
def inicio():
    return render_template("inicio.html")

@app.route("/privacidad")
def privacidad():
    return render_template("privacidad.html")

@app.route("/micv")
def micv():
    return render_template("micv.html")

@app.route("/registro")
def registro():
    return render_template("registro.html")

@app.route("/registro_empresa")
def registro_empresa():
    return render_template("registro_empresa.html")

@app.route("/registrar_negocio_proceso", methods=['POST'])
def registrar_negocio_proceso():
    global datos_empresa_global
    datos_empresa_global = {
        "responsable": request.form.get("responsable"),
        "email": request.form.get("email"),
        "nombre_comercial": request.form.get("nombre_comercial"),
        "rfc": request.form.get("rfc"),
        "telefono": request.form.get("telefono"),
        "razon_social": request.form.get("razon_social"),
        "localidad": request.form.get("localidad"),
        "sector": request.form.get("sector"),
        "empleados": request.form.get("empleados"),
        "vacantes_est": request.form.get("vacantes_est")
    }
    return redirect(url_for('minegocio'))

@app.route("/minegocio")
def minegocio():
    return render_template("minegocio.html", empresa=datos_empresa_global)

@app.route("/login")
def login():
    return render_template("login.html")

@app.route("/negocio/<int:negocio_id>")
def detalle_negocio(negocio_id):
    return render_template("detalle_negocio.html", id=negocio_id)

@app.route("/ayuda")
def ayuda():
    return render_template("ayuda.html")

@app.route("/miperfil")
def miperfil():
    return render_template("miperfil.html")

if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=5000)