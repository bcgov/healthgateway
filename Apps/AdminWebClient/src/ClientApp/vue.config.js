module.exports = {
  transpileDependencies: ["vuetify"],
  publicPath: process.env.NODE_ENV === "production" ? "/admin/" : "/",
  assetsDir: process.env.NODE_ENV === "production" ? "admin/" : ""
};
