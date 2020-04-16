<template>
  <section>
    <NarBar></NarBar>
    <router-view/>
  </section>
</template>

<script>
import navbar from '../components/NavBar';
import { GetRouter } from '../utils/token';

export default {
  name: 'index',
  components: {
    NarBar: navbar
  },
  mounted () {
    if (GetRouter()) {
      this.$router.replace(GetRouter());
    } else {
      this.$router.replace(this.$store.getters.addRouters[0].children[0].path);
    }
  }
};
</script>

<style scoped>

</style>
